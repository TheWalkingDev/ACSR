using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TestTCPShared;

namespace ACSR.Core.Networking.SimpleTCP
{
    public class SimpleTCPServer
    {

        private TcpListener tcpListener;
        private Thread listenThread;

        public delegate void ClientMessage(SocketReaderWriter AClient, string AMessage);
        public delegate void ClientEvent(SocketReaderWriter AClient);

        public event ClientMessage OnClientMessage;
        public event ClientEvent OnClientConnect;
        public event ClientEvent OnClientDisconnect;


        public SimpleTCPServer(int APort)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, APort);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);

            }
        }

       

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            //NetworkStream clientStream = tcpClient.GetStream();
            var rw = new SocketReaderWriter(tcpClient);
            if (OnClientConnect != null)
                OnClientConnect(rw);


            byte[] message = new byte[4096];
            //int bytesRead;
            //char nullChar = new char();
            StringBuilder sb = new StringBuilder();
            ASCIIEncoding encoder = new ASCIIEncoding();
            while (tcpClient.Connected)
            {
                //bytesRead = 0;

                try
                {
                    //blocks until a client sends a message                    
                    string msg = rw.ReadSocketMessage();
                    try
                    {

                        if (!string.IsNullOrEmpty(msg) && OnClientMessage != null)
                            OnClientMessage(rw, msg);
                        else if (!tcpClient.Connected)
                        {
                            //the client has disconnected from the server
                            break;
                        }

                    }
                    finally
                    {
                        // decrements the channel counter without sending a message
                        rw.EndChannel();
                        if (tcpClient.Connected)
                            rw.EndChannelMessage();
                    }
                        
                }
                catch
                {
                    //a socket error has occured
                    break;
                }


                //message has successfully been received
                
                //  System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
            }
            tcpClient.Close();
            if (OnClientDisconnect != null)
                OnClientDisconnect(rw);
        }
    }
}