using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ACSR.Core.Networking.IOCP
{
    public class IOCPReaderWriter : IIOCPSocketController, IIOCPSocketContext
    {
        public IOCPMessageQueue MessageQueue { get; set; }
        private Socket _socket;
        public delegate void MessageEvent(object sender, byte[] Data);

        public List<byte[]> _sendQueue;
        public delegate IOCPReaderWriter GetReaderWriterEvent(Socket socket);
        public event MessageEvent OnMessage;
        public event SocketIOEvent OnDisconnect;
        public void DoDisconnectSocket()
        {
            if (OnDisconnect != null)
            {
                OnDisconnect(this, this);
            }
        }
        public IOCPReaderWriter(Socket socket)
        {
            _sendQueue = new List<byte[]>();
            _socket = socket;
            BeginReceiveMessages();        
        }
        public Socket Socket
        {
            get
            {
                return _socket;
            }
        }

        public IOCPReaderWriter IO
        {
            get { return this; }
        }

        public void SendMessage(string Message)
        {
            SendBuffer(ASCIIEncoding.ASCII.GetBytes(Message));
        }

        private int _sendCounter = 0;
        private void SendDirect(byte[] ABuffer)
        {
            var t = new ByteTransmitter(_socket, ABuffer, this);
            t.OnSendComplete += new SendCompleteEvent(t_OnSendComplete);
            t.SendMessage();            
        }

        public void SendBuffer(byte[] ABuffer)
        {
            lock (_sendQueue)
            {
                _sendQueue.Add(ABuffer);    
                if (_sendQueue.Count == 1)
                {
                    SendDirect(ABuffer);    
                }
            }                        
        }

        void t_OnSendComplete(object sender, byte[] Buffer)
        {
            byte[] buf = null;
            lock (_sendQueue)
            {
                if (_sendQueue.Count > 0)
                {
                    _sendQueue.RemoveAt(0);
                    if (_sendQueue.Count > 0)
                    {
                        buf = _sendQueue[0];
                        SendDirect(buf);                        
                    }
                }                
            }
        }

        protected virtual void InternalMessageComplete(byte[] data)
        {
            MessageQueue.Put(this, data);
        }

        private void BeginReceiveMessages()
        {
            var recv = new ByteReceiver(_socket, this);
            recv.OnReceived += new ReceivedEvent(recv_OnReceived);
            recv.ReceiveMessage();
        }

        void recv_OnReceived(byte[] Data)
        {
            try
            {
                InternalMessageComplete(Data);
                if (OnMessage != null)
                    OnMessage(this, Data);
                
            }
            finally
            {
                BeginReceiveMessages();    
            }
            
        }
    }

    public delegate void SocketIOEvent(object sender,  IIOCPSocketContext context);
    

    

}
