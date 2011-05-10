using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    public delegate void ReceivedEvent(byte[] Data);

    public delegate void SendCompleteEvent(object sender, byte[] Buffer);
    public class ByteTransmitter
    {
        public event SendCompleteEvent OnSendComplete;
        private byte[] _buffer;
        private Socket _socket;
        private int _position = 0;
        public event ReceivedEvent OnReceived;
        private IIOCPSocketController _io;
        public ByteTransmitter(Socket socket, byte[] buffer, IIOCPSocketController IO)
        {
            _io = IO;
            _socket = socket;
            _buffer = buffer;
        }
        public ByteTransmitter(Socket socket, int Size, IIOCPSocketController IO)
        {
            _io = IO;
            _socket = socket;
            _buffer = new byte[Size];
        }

        public void BeginReceive()
        {
            _socket.BeginReceive(_buffer, _position, _buffer.Length - _position, SocketFlags.None, new AsyncCallback(OnReceiveMessage), this);
        }
        private void OnReceiveMessage(IAsyncResult result)
        {
            try
            {
                var count = _socket.EndReceive(result);
                _position += count;
                if (_position == _buffer.Length)
                {
                    if (OnReceived != null)
                        OnReceived(_buffer);
                }
                else
                {
                    BeginReceive();
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    if (_io != null)
                        _io.DoDisconnectSocket();
                }
            }
        }
        private void BeginSendMessage()
        {
            try
            {

                var bSize = BitConverter.GetBytes(_buffer.Length);
                var bTotal = new byte[bSize.Length + _buffer.Length];

                Buffer.BlockCopy(bSize, 0, bTotal, 0, bSize.Length);
                Buffer.BlockCopy(_buffer, 0, bTotal, bSize.Length, _buffer.Length);
                _buffer = bTotal;

                _socket.BeginSend(_buffer, _position, _buffer.Length - _position, SocketFlags.None,
                                  new AsyncCallback(OnSendMessage), this);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    if (_io != null)
                        _io.DoDisconnectSocket();
                }
            }
        }
        internal void SendMessage()
        {

            BeginSendMessage();
        }
        private void OnSendMessage(IAsyncResult result)
        {
            try
            {
                var count = _socket.EndSend(result);
                _position += count;
                if (_position != _buffer.Length)
                {
                    BeginSendMessage();
                }
                else
                {
                    if (OnSendComplete != null)
                    {
                        OnSendComplete(this, _buffer);
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    if (_io != null)
                        _io.DoDisconnectSocket();
                }
            }
        }
    }

}
