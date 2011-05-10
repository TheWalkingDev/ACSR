using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TestTCPShared;

namespace ACSR.Core.Networking.SimpleTCP
{
    public class SimpleTCPClient : IDisposable
    {
        private IPAddress _ip;
        private int _port;
        private TcpClient _client;
        private SocketReaderWriter _io;

        public SimpleTCPClient(string AIP, int APort)
            : this(IPAddress.Parse(AIP), APort)
        {
        }
        public bool IsChannelOpen
        {
            get
            {
                return _io.IsChannelOpen;
            }
        }

        public SimpleTCPClient(IPAddress AIPAddress, int APort)
        {
            _ip = AIPAddress;
            _port = APort;
        }
        public void SendMessage(string AMessage)
        {
            _io.WriteSocketMessage(AMessage);
        }
        public string ReceiveMessage()
        {
            return _io.ReadSocketMessage();
        }

        private void SetConnected(bool AConnected)
        {
            if (AConnected)
            {
                if (_client == null || !_client.Connected)
                {
                    _client = new TcpClient();
                    IPEndPoint serverEndPoint = new IPEndPoint(_ip, _port);
                    _client.Connect(serverEndPoint);
                    _io = new SocketReaderWriter(_client);
                }
            }
            else
            {
                if (_client != null)
                {
                    if (_client.Connected)
                        _client.Close();
                    _client = null;
                    _io = null;
                }
            }
        }
        public bool Connected
        {
            get
            {
                return (_client != null && _client.Connected);
            }
            set
            {
                SetConnected(value);
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            SetConnected(false);
        }

        #endregion
    }
}
