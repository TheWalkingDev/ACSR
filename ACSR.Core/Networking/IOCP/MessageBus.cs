// DEPRECATED, REVERT TO IOCPClient and IOCPServer
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using ACSR.Core.System.Reflection;
using ACSR.Core.System.Threading;

namespace ACSR.Core.Networking.IOCP
{

    public class QueueProcessor
    {
        private MessageBus _bus;
        private Messages _messages;
        public event MessageEvent OnMessage;
        private bool _terminated = false;
        public QueueProcessor(MessageBus bus, string QueueName) : this(bus, QueueName, 1)
        {            
        }

        public QueueProcessor(MessageBus bus, string QueueName, int ThreadPoolSize)
        {
            _bus = bus;
            _messages = bus.GetQueue(QueueName, true);
            for (int i = 1; i <= ThreadPoolSize;i++)
            {
                ThreadUtils.FastThread(Handler).Start();
            }                
        }

        public void Handler(Thread AThread, params object[] args)
        {
            while (!_terminated)
            {

                if (_messages.WaitForMessage(1000))
                {
                    foreach  (var message in _messages.EnumTransactions())
                    {
                        if (message.MsgType == MessageType.Receive)
                        {
                          //  _Requests[message.MessageId] = message;
                            var resMessage = _messages.GetMessage(false, message.MessageId);
                            /*if (resMessage == null)
                            {
                                resMessage = new Message();
                                resMessage.MessageBody = ASCIIEncoding.ASCII.GetBytes("Message not found");
                                resMessage.QueueName = message.QueueName;
                                resMessage.MessageId = -1;
                            }
                            if (resMessage != null)
                            {
                                message.Controller.SendMessage(resMessage);
                            }*/
                            if (resMessage == null)
                                _messages.PutMessage(message);
                            else
                            {
                                resMessage.MsgType = MessageType.Send;
                                message.Controller.SendMessage(resMessage);
                            }
                        }
                        else
                        {
                            if (OnMessage != null)
                                OnMessage(this, message);
                            _messages.CommitMessage(message);
                        }
                    }
                }
            }
        }
    }
   
    public class MessageClient
    {
         private TcpClient _client;
         private MessageBus _bus;
        private SocketController _controller;
         public MessageClient(string host, int port)
        {
            _bus = new MessageBus();
            _client = new TcpClient();
            _client.Connect(host, port);
            var ctx = new SocketController(_client.Client);
             _controller = ctx;
             
            ctx.OnMessage += new MessageEvent(ctx_OnMessage);
        }
         void ctx_OnMessage(object sender, Message Message)
         {
             var q = _bus.GetQueue(Message.QueueName, true);
             q.PutMessage(Message);
         }
         public void SendMessage(Message msg)
         {
             _controller.SendMessage(msg);
         }
         public Message GetMessage(Message msg, int Timeout)
         {
             msg.MsgType = MessageType.Receive;
             var q = _bus.GetQueue(msg.QueueName, true);
             SendMessage(msg);

             msg = q.GetMessage(msg.MessageId, false, Timeout);
             if (msg != null && msg.MessageId == -1)
                 msg = q.GetMessage(msg.MessageId, false, Timeout);
             return msg;
         }

         public MessageBus MessageBus
         {
             get
             {
                 return _bus;
             }
         }
    }
    public class MessageServer
    {
        private TcpListener _server;
        private MessageBus _bus;
        public MessageBus MessageBus
        {
            get
            {
                return _bus;
            }
        }
        public MessageServer(int Port)
        {
            _bus = new MessageBus();
            _server = new TcpListener(Port);
            _server.Start();
            _server.BeginAcceptSocket(new AsyncCallback(OnConnect), _server);
        }
        void OnConnect(IAsyncResult res)
        {
            var server = (TcpListener)res.AsyncState;
            var socket = server.EndAcceptSocket(res);
            var ctx = new SocketController(socket);
            ctx.OnMessage += new MessageEvent(ctx_OnMessage);
            server.BeginAcceptSocket(new AsyncCallback(OnConnect), server);
        }
        void ctx_OnMessage(object sender, Message Message)
        {
            var q = _bus.GetQueue(Message.QueueName, true);
            q.PutMessage(Message);
        }

    }
    public class MessageBus
    {
        
        private Dictionary<string, Messages> _messages;
        
        public MessageBus()
        {
            _messages = new Dictionary<string, Messages>();
        }

        public Messages GetQueue(string Queue, bool AutoCreate)
        {
            Messages ret = null;
            
            if (!_messages.TryGetValue(Queue, out ret))
            {
                if (AutoCreate)
                {
                    ret = new Messages(this, Queue);
                    _messages[Queue] = ret;
                }
            }
            return ret;
        }
    }

    public class Messages : Dictionary<int, Message>
    {
        private MessageBus _bus;
        private ManualResetEvent _event;
        private List<Message> _Transactions;
        private Dictionary<int, Message> _Requests;
        private string _name;
        public Messages(MessageBus bus, string Name)
        {
            _bus = bus;
            _event = new ManualResetEvent(false);
            _Transactions = new List<Message>();
            _Requests = new Dictionary<int, Message>();
            _name = Name;
        }
        public bool WaitForMessage(int TimeOut)
        {
            return _event.WaitOne(TimeOut);
        }

        public IEnumerable<Message> EnumTransactions()
        {
            Message trans = null;
            do
            {
                lock (_Transactions)
                {
                    if (_Transactions.Count > 0)
                    {
                        trans = _Transactions[0];
                        _Transactions.RemoveAt(0);
                    }
                }

                yield return trans;
            }
            while (trans != null);
            yield return null;
        }
        

        public void CommitMessage(Message msg)
        {
            lock (this)
            {
                if (!this.ContainsKey(msg.MessageId))
                {
                    this[msg.MessageId] = msg;    
                }                
            }
            _event.Set();
        }

        public void PutMessage(Message msg)
        {

            if (msg.MsgType == MessageType.Receive)
            {
                var responseMsg = GetMessage(msg.MessageId, true, 0);
                if (responseMsg == null)
                {
                    _Requests[msg.MessageId] = msg;
                }
            }
            else
            {
                if (_Requests.ContainsKey(msg.MessageId))
                {
                    var responseMsg = GetMessage(msg.MessageId, true, 0);

                }
                lock (_Transactions)
                {
                    _Transactions.Add(msg);
                }
                _event.Set();
            }            
        }
        internal Message GetMessage(bool Peek, int TimeOut)
        {
            Message ret = null;

            lock (this)
            {
                if (this.Count > 0)
                {
                    ret = this.Values.First();
                    if (!Peek && ret != null)
                    {
                        this.Remove(ret.MessageId);
                    }
                }
            }
            if (ret == null && TimeOut > 0)
            {
                if (_event.WaitOne(TimeOut))
                {
                    return GetMessage(Peek, 0);
                }
            }
            return ret;

        }

        internal Message GetMessage(int Id, bool Peek, int TimeOut)
        {
            Message ret = null;
            
            lock (this)
            {
                if (this.TryGetValue(Id, out ret))
                {
                    if (!Peek)
                    {
                        this.Remove(Id);
                    }
                }
            }
            if (ret == null && TimeOut > 0)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                do
                {
                    if (_event.WaitOne(TimeOut))
                    {

                        lock (this)
                        {
                            if (this.TryGetValue(Id, out ret))
                            {
                                if (!Peek)
                                {
                                    this.Remove(Id);
                                }
                            }
                        }
                    }

                } while (ret == null && sw.ElapsedMilliseconds < TimeOut);
            }
            
            return ret;
        }
    }

    

    internal class Context
    {
        private SocketController _controller;
        private ByteTransmitter _message = null;
        private ByteTransmitter _size = null;
      
        public Context(SocketController controller)
        {
            _controller = controller;
            _size = new ByteTransmitter(controller.Socket, 4);
            _size.OnReceived += new ReceivedEvent(_size_OnReceived);
            _size.BeginReceive();
        }

        void _size_OnReceived(byte[] Data)
        {
            var size = BitConverter.ToInt32(Data, 0);
            _message = new ByteTransmitter(_controller.Socket, size);
            _message.OnReceived += new ReceivedEvent(_message_OnReceived);
            _message.BeginReceive();
        }

        void _message_OnReceived(byte[] Data)
        {
            _controller.MessageReceived(Data);
        }
    }

    

    public delegate void MessageEvent(object sender, Message Message);
    
    internal class SocketController
    {
        private Socket _socket;
        public Socket Socket
        {
            get
            {
                return _socket;
            }
        }
        public SocketController(Socket socket)
        {
            _socket = socket;
            var ctx = new Context(this);
        }

        public event MessageEvent OnMessage;
        internal void SendMessage(Message AMessage)
        {
            var ms = new MemoryStream();
            AMessage.Save(ms);
            byte[] data = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(data, 0, data.Length);
            new ByteTransmitter(Socket, data).SendMessage();
        }

        internal void MessageReceived(byte[] Message)
        {
            var ms = new MemoryStream(Message);
            ms.Position = 0;            
            Message msg = new Message();
            msg.Controller = this;
            msg.Load(ms);
            if (OnMessage != null)
                OnMessage(this, msg);
            new Context(this);
        }
    }

    public class Message
    {
        private Stream _stream;
        public int MessageId { get; set; }
        internal MessageType MsgType { get; set; }
        public string QueueName { get; set; }
        public byte[] MessageBody { get; set; }
        internal SocketController Controller { get; set; }
        private byte[] ReadBytes()
        {
            byte[] size = new byte[4];
            _stream.Read(size, 0, 4);
            int iSize = BitConverter.ToInt32(size, 0);
            if (iSize == 0)
            {
                return null;
            }
            else
            {

                byte[] s = new byte[iSize];
                _stream.Read(s, 0, iSize);
                return s;
            }
        }
    
        private void WriteBytes(byte[] data)
        {
            //_stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            if (data == null)
                WriteInt(0);
            else
            {
                WriteInt(data.Length);
                _stream.Write(data, 0, data.Length);
            }
        }
        private int ReadInt()
        {
            byte[] size = new byte[4];
            _stream.Read(size, 0, 4);
            return BitConverter.ToInt32(size, 0);
        }
        private void WriteInt (int value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 4);
        }
        private void WriteString(string s)
        {
            WriteBytes(ASCIIEncoding.ASCII.GetBytes(s));
        }
        private string ReadString()
        {
            return ASCIIEncoding.ASCII.GetString(ReadBytes());
        }

        public void Load(Stream Stream)
        {
            _stream = Stream;
            MessageId = ReadInt();
            MsgType = (MessageType) ReadInt();
            QueueName = ReadString();
            MessageBody = ReadBytes();
        }
        public void Save(Stream Stream)
        {
            _stream = Stream;
            WriteInt(MessageId);
            WriteInt((int)MsgType);
            WriteString(QueueName);
            WriteBytes(MessageBody);
        }
    }

    [Serializable()]
    internal class MyMessage : ISerializable
    {
        public int MessageId { get; set; }
        internal int MessageSize { get; set; }
        internal int QueueNameSize { get; set; }
        public MessageType MsgType { get; set; }
        internal string QueueName { get; set; }
        public static int GetSize()
        {
            return 4;
        }
        public MyMessage(SerializationInfo info, StreamingContext ctxt)
        {
            /*MessageId =  (int)info.GetValue("MessageId", typeof(int));
            MsgType = (MessageType)info.GetValue("MsgType", typeof(int));
            MessageSize = (int)info.GetValue("MessageSize", typeof(int));
            QueueNameSize = (int)info.GetValue("QueueNameSize", typeof(int));
            info.Get*/
            
            SerializationWriter sw = SerializationWriter.GetWriter();

        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            /*info.AddValue("MessageId", MessageId);
            info.AddValue("MsgType", (int)MsgType);
            info.AddValue("MessageSize", MessageSize);
            info.AddValue("QueueNameSize", QueueNameSize);*/

        }
    }
    enum MessageType
    {
        Send = 1,
        Receive = 2
    }

    


}
