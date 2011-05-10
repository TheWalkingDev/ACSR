using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACSR.Core.Networking.IOCP.RPCServices
{
    public class RPCService
    {

        private IOCPMessageQueueRPC _Queue;
        public IOCPMessageQueueRPC Queue
        {
            get
            {
                return _Queue;
            }
        }
        public RPCService(IOCPMessageQueueRPC Queue)
        {
            _Queue = Queue;
        }
    }

    public class RPCClientService : RPCService
    {
        private IOCPReaderWriter _io;
        public IOCPReaderWriter IO
        {
            get
            {
                return _io;
            }
        }
        public RPCClientService(IOCPMessageQueueRPC Queue, IOCPReaderWriter IO) : base(Queue)
        {
            _io = IO;
        }
    }

    public class SendFile : RPCClientService
    {
        public int BufferSize = 4000000;

        public SendFile(IOCPMessageQueueRPC Queue, IOCPReaderWriter IO) : base(Queue, IO)
        {
        }

        public IEnumerable<long> Send(string LocalFile, string RemoteFile)
        {
            using (FileStream fs = new FileStream(LocalFile, FileMode.Open))
            {
                foreach (int i in Send(RemoteFile, fs))
                {
                    yield return i;
                }
            }
        }

        public IEnumerable<long> Send(string RemoteFile, Stream Stream)
        {
            var ret = Queue.Execute(IO, "ACSR.Core.Networking.IOCP.RPCServices.ReceiveFile.BeginSendFile", RemoteFile);
            byte[] buffer = null;
            var total = Stream.Length;
            var left = total;
            while (left > 0)
            {
                Int32 actualBuffer = BufferSize;
                if (left < actualBuffer)
                {
                    actualBuffer = (int)left;
                }
                buffer = new byte[actualBuffer];
                Stream.Read(buffer, 0, buffer.Length);
                ret = Queue.Execute(IO, "ACSR.Core.Networking.IOCP.RPCServices.ReceiveFile.SendFileData", RemoteFile, buffer);
                left -= actualBuffer;
                yield return left;
                
            }
            Queue.Execute(IO, "ACSR.Core.Networking.IOCP.RPCServices.ReceiveFile.EndSendFile", RemoteFile);
        }
    }

    public delegate Stream SendFileEvent(string FileName);
    public delegate void ReceiveFileCompleteEvent(string FileName, Stream Stream);    
    public delegate byte[] ReceiveFileDataEvent(string FileName, Stream Stream, byte[] Data);

    public class ReceiveFiles : RPCService
    {
        public event SendFileEvent OnSendFile;
        public event ReceiveFileCompleteEvent OnReceiveFileComplete;
        public event ReceiveFileDataEvent OnReceiveFileData;
        public ReceiveFiles(IOCPMessageQueueRPC Queue)
            : base(Queue)
        {
            foreach (var session in Queue.RegisterRPCHandlers())
            {
                Queue.RegisterRPCHandler(session, new ReceiveFile(this));
            }
        }
        internal byte[] RaiseReceiveFileDataEvent(string FileName, Stream Stream, byte[] Data)
        {
            if (OnReceiveFileData != null)
                return OnReceiveFileData(FileName, Stream, Data);
            else
            {
                return Data;
            }

        }
        public  void RaiseReceiveFileCompleteEvent(string FileName, Stream Stream)
        {
            if (OnReceiveFileComplete != null)
                OnReceiveFileComplete(FileName, Stream);
        }
        internal Stream RaiseGetSendFileEvent(string FileName)
        {
            if (OnSendFile != null)
                return OnSendFile(FileName);
            else
            {
                throw new Exception("OnSendFile event not assigned on server");
            }
        }
    }

    internal class ReceiveFile : IRpcHandler
    {
        private Dictionary<string, Stream> _files;
        private ReceiveFiles _fileHandlers;
        public ReceiveFile(ReceiveFiles Files) 
        {
            _fileHandlers = Files;
         
        }

        private Dictionary<string, Stream> Files
        {
            get
            {
                if (_files == null)
                {
                    _files = new Dictionary<string, Stream>();
                }
                return _files;
            }
        }

        private Stream GetFileStream(string FileName, bool CreateNew)
        {
            Stream fs;
            if (CreateNew)
            {
                fs = _fileHandlers.RaiseGetSendFileEvent(FileName);
                Files[FileName] = fs;                
            }
            else
            {
                if (!Files.TryGetValue(FileName, out fs))
                {
                    throw new Exception("Stream was not created for:" + FileName );
                }
            }
            return fs;
        }
        public object[] BeginSendFile(IRpcContext context, string FileName)
        {            
            GetFileStream(FileName, true);
            return new object[] { "success" };
        }
        public object[] SendFileData(IRpcContext context, string FileName, byte[] Data)
        {
            var fs = GetFileStream(FileName, false);
            Data = _fileHandlers.RaiseReceiveFileDataEvent(FileName, fs, Data);

            fs.Write(Data, 0, Data.Length);
            return new object[] { "success" };
        }
        public object[] EndSendFile(IRpcContext context, string FileName)
        {
            var fs = GetFileStream(FileName, false);
            _fileHandlers.RaiseReceiveFileCompleteEvent(FileName, fs);
            fs.Close();
            Files.Remove(FileName);
            return new object[] { "success" };
        }

        public object GetInstance()
        {
            return this;
        }
    }
}