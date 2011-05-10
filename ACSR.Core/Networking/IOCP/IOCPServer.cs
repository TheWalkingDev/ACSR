using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{


    public class IOCPServer : IOCPSocket, IDisposable
    {
        private TcpListener _server;
        IOCPServerClients _Clients;
        public IOCPMessageQueue MessageQueue { get; set; }
        public void GetClients(List<IIOCPSocketContext> Clients)
        {
            lock (_Clients)
            {
                foreach (var client in _Clients)
                {
                    Clients.Add(client);
                }
            }
        }

        public IOCPServer(int Port)
        {
            _Clients = new IOCPServerClients();
            _server = new TcpListener(Port);
        }
        
        public void Start()
        {
            _server.Start();
            _server.BeginAcceptSocket(new AsyncCallback(OnConnect), _server);
        }
        public void Stop()
        {
            var clients = new List<IIOCPSocketContext>();
            GetClients(clients);
            
            _server.Stop();
            foreach (var client in clients)
            {
                client.Socket.Disconnect(false);
            }
        }

        public event IOCPReaderWriter.GetReaderWriterEvent OnGetReaderWriter;
        protected virtual IOCPReaderWriter InternalGetReaderWriter(Socket socket)
        {
            if (OnGetReaderWriter != null)
                return OnGetReaderWriter(socket);
            return new IOCPReaderWriter(socket);            
        }

        void OnConnect(IAsyncResult res)
        {
            try
            {
                var server = (TcpListener) res.AsyncState;
                var socket = server.EndAcceptSocket(res);

                server.BeginAcceptSocket(new AsyncCallback(OnConnect), server);
                var rw = InternalGetReaderWriter(socket);
                InitSocketIO(rw);
                rw.MessageQueue = this.MessageQueue;
                rw.OnDisconnect += new SocketIOEvent(rw_OnDisconnect);
                RaiseEventConnect(rw);
                lock (_Clients)
                {
                    _Clients.Add(rw);
                }
            }
            catch(ObjectDisposedException e)
            {
                
            }
        }

        void rw_OnDisconnect(object sender, IIOCPSocketContext context)
        {
            lock (_Clients)
            {
                _Clients.Remove(context);
            }
            
        }

      
        public void Dispose()
        {
            Stop();
        }
    }
    public class IOCPServerClients : List<IIOCPSocketContext>
    {
        
    }


}
