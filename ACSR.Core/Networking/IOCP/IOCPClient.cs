using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    public class IOCPClient : IOCPSocket
    {
        private TcpClient _client;
        public TcpClient Client
        {
            get
            {
                return _client;
            }
        }
        public IOCPMessageQueue MessageQueue { get; set; }
        
        public IOCPClient ()
        {
            _client = new TcpClient();
       
        }
       
        public Type SocketReaderWriterType { get; set; }
        public IOCPReaderWriter ReaderWriter { get; set; }

        public event IOCPReaderWriter.GetReaderWriterEvent OnGetReaderWriter;
        protected virtual IOCPReaderWriter InternalGetReaderWriter(Socket socket)
        {
            if (OnGetReaderWriter != null)
                return OnGetReaderWriter(socket);
            return new IOCPReaderWriter(socket);
        }

        public void Connect(string Host, int Port)
        {
            _client.Connect(Host, Port);
            var rw = InternalGetReaderWriter(_client.Client);
            InitSocketIO(rw);
            
            rw.MessageQueue = this.MessageQueue;
            ReaderWriter = rw;
            RaiseEventConnect(rw);
        }

    }
}
