using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    public class ByteReceiver
    {
        private Socket _socket;
        private ByteTransmitter _message = null;
        private ByteTransmitter _size = null;
        public event ReceivedEvent OnReceived;
        public event SocketIOEvent OnDisconnect;
        private IIOCPSocketController _io;
        public ByteReceiver(Socket socket, IIOCPSocketController IO)
        {
            _io = IO;
            _socket = socket;
            _size = new ByteTransmitter(socket, 4, IO);
            
            _size.OnReceived += new ReceivedEvent(_size_OnReceived);
            
        }
        public void ReceiveMessage()
        {
            _size.BeginReceive();
        }

        void _size_OnReceived(byte[] Data)
        {
            var size = BitConverter.ToInt32(Data, 0);
            _message = new ByteTransmitter(_socket, size, _io);
            _message.OnReceived += new ReceivedEvent(_message_OnReceived);
            _message.BeginReceive();
        }

        void _message_OnReceived(byte[] Data)
        {
            if (OnReceived != null)
                OnReceived(Data);
        }
    }
}
