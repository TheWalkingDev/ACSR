using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TestTCPShared
{
    public class SocketReaderWriter : IDisposable
    {
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        private ASCIIEncoding _enc;

        public readonly string CSysMessage = "{BAF65E82-8F96-4bb2-A6F6-17E7A84BB56C}";
        

        public TcpClient ClientSocket
        {
            get
            {
                return _client;
            }
        }

        public SocketReaderWriter(TcpClient AClient)
        {
            _client = AClient;
            _reader = new StreamReader(AClient.GetStream());
            _writer = new StreamWriter(AClient.GetStream(), Encoding.UTF8);
            _writer.AutoFlush = true;
            _enc =  new ASCIIEncoding();
        }

        private int _channelCount = 0;
        
        public void BeginChannel()
        {
            _channelCount++;
        }
        public void EndChannel()
        {
            _channelCount--;
        }
        public void BeginChannelMessage()
        {            
            BeginChannel();
            _internalWriteSocketMessage(CSysMessage);
            _internalWriteSocketMessage(((int)SysMessageType.BeginChannel).ToString());
           // WriteSocketMessage(_channelCount.ToString());
        }
        public void EndChannelMessage()
        {
            EndChannel();
            _internalWriteSocketMessage(CSysMessage);
            _internalWriteSocketMessage(((int)SysMessageType.EndChannel).ToString());
            //WriteSocketMessage(_channelCount.ToString());
        }
        private void CheckSysMessage(string AMessage)
        {
            string msgType = ReadSocketMessage();
            SysMessageType t = (SysMessageType) Int32.Parse(msgType);
            switch (t)
            {
                case SysMessageType.BeginChannel:
                    {
                        BeginChannel();
                    }
                    break;
                case SysMessageType.EndChannel:
                    {
                        EndChannel();
                    }
                    break;
            }            
        }
        public bool IsChannelOpen
        {
            get
            {
                return _channelCount > 0;
            }
        }


        public string ReadSocketMessage()
        {
            string msg = null;
            while (msg == null)
            {
                string sSize;
                var sb = new StringBuilder();
                StreamReader sr = _reader;
                {

                    sSize = sr.ReadLine();
                }
                char[] data = new char[Int32.Parse(sSize)];
                int bytesRead = _reader.ReadBlock(data, 0, data.Length);
                var bytes = _enc.GetBytes(data);
                msg = _enc.GetString(bytes, 0, bytesRead);
                if (msg == CSysMessage)
                {
                    CheckSysMessage(msg);
                    //if (_channelCount == 0)
                        return null;
                }
            }
            return msg;
        }
        public void SendMessage(string AMessage)
        {
            
            WriteSocketMessage(AMessage);
        }
        public string ReceiveMessage()
        {
            return ReadSocketMessage();
        }
        private void _internalWriteSocketMessage(string AMessage)
        {
            if (string.IsNullOrEmpty(AMessage))
                return;
            StreamWriter s = _writer;
            {
                s.WriteLine(AMessage.Length.ToString());
                s.Write(AMessage);
            }
            
        }
        public void WriteSocketMessage(string AMessage)
        {
            BeginChannel();
            _internalWriteSocketMessage(AMessage);
        }




        #region Implementation of IDisposable

        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();
        }

        #endregion
    }

    public enum SysMessageType
    {
        Unknown = 0,
        BeginChannel = 1,
        EndChannel = 2


    }
}
