using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using ACSR.Core.Networking.IOCP.RPCServices;

namespace ACSR.Core.Networking.IOCP
{
    public class IOCPMessageQueueRPC : IOCPMessageQueue
    {
        private List<IOCPRPCCall> _responseQ;
        //private List<object> _handlers;
        private SessionList _sessions;
       // private MethodMap _methodMap;

        public delegate void MessageEvent(object sender, IRpcContext context, string message);

        public event MessageEvent OnMessage;
        public IOCPMessageQueueRPC() : this(5)
        {            
        }

        public IOCPMessageQueueRPC(int PooledSessions) : base()
        {
            _responseQ = new List<IOCPRPCCall>();
          //  _handlers = new List<object>();
            _sessions = new SessionList();
            for (int i = 1; i <= PooledSessions; i++ )
            {
                _sessions.Add(new Session());
            }
            _sessions.InitSessions();
           // _methodMap = _sessions[0];
        }
        public IEnumerable<ISession> RegisterRPCHandlers()
        {
            foreach (var session in _sessions)
            {
                yield return session;
            }
        }
        public void RegisterRPCHandler(ISession session, IRpcHandler handler)
        {
                foreach (var metho in handler.GetType().GetMethods())
                {
                    string fullName = string.Format("{0}.{1}", handler.GetType().FullName, metho.Name);
                    object instance = handler.GetInstance();
                    ((Session)session)[metho.Name] = new MethodHandlerContext(instance, metho);
                    ((Session)session)[fullName] = new MethodHandlerContext(instance, metho);
                }
            //_handlers.Add(handler);
        }

        private void Serialize(Stream AStream, object AInstance)
        {
            //  XmlSerializer ser = GetSerializer(AInstance.GetType());
            //  ser.Serialize(AStream, AInstance);

            BinaryFormatter myBF = new BinaryFormatter();
            myBF.Serialize(AStream, AInstance);

        }
        private object DeSerialize(Stream AStream, Type AType)
        {
            //XmlSerializer ser = GetSerializer(AType);
            //return ser.Deserialize(AStream);         

            BinaryFormatter myBF = new BinaryFormatter();
            return myBF.Deserialize(AStream);
        }
        public IOCPRPCCall Execute(IOCPReaderWriter Context, string Name, params object[] args)
        {
            var call = new IOCPRPCCall();
            call.Method = Name;
            foreach (object p in args)
            {
                call.Params.Add(p);
            }
            return Execute(Context, call);

        }
       public IOCPRPCCall GetResponse(IOCPRPCCall caller)
        {
            IOCPRPCCall res = null;
            while (res == null)
            {
                ProcessMessages(1000);
                lock (_responseQ)
                {
                    foreach (var r in _responseQ)
                    {
                        if (r.Id == caller.Id)
                        {
                            _responseQ.Remove(r);
                            res = r;
                            break;
                        }

                    }
                }
                //ProcessMessages();
                //_responseEvent.WaitOne(1000);    
            }
            return res;
        }
       public IOCPRPCCall Execute(IOCPReaderWriter Context, IOCPRPCCall call)
       {

           //XmlSerializer ser = new XmlSerializer(call.GetType());
           var ms = new MemoryStream();
           //ser.Serialize(ms, call);
           Serialize(ms, call);
           // var msg = ASCIIEncoding.ASCII.GetString(ms.GetBuffer());            
           // SendMessage(msg);

           var buf = new byte[ms.Length];
           
           //Buffer.BlockCopy(ms.GetBuffer(), 0, buf, 0, (int) ms.Length);
           ms.Position = 0;
           ms.Read(buf, 0, buf.Length);
           ms.Position = 0;

           /*
           /// TEST CODE
           DeSerialize(ms, call.GetType());
           ms.Position = 0;
           // END TEST CODE
           */
           Context.SendBuffer(buf);

           if (call.Response == (int)ResponseType.None)
           {
               call = GetResponse(call);
               if (call.Response == (int)ResponseType.Error)
               {
                   var err = new StringBuilder();
                   err.AppendLine("RPC Error:");
                   
                   foreach (var param in call.Params)
                   {
                       err.AppendLine(param.ToString());
                   }
                   //throw new Exception(string.Format("RPC Error: {0}", call.Params[0]));
                   throw new Exception(err.ToString());
               }
           }
           else
           {
               call = null;
           }
           return call;

       }
        public void BroadCastMessage(IOCPServerClients Clients, string Message )
        {
             foreach (var client in Clients)
             {
                 SendMessage(client.IO, Message);
             }
        }

        public void SendMessage(IOCPReaderWriter IO, string Message)
        {
            var call = new IOCPRPCCall();
            call.Response = (int) ResponseType.Message;
            call.Params.Add(Message);
            Execute(IO, call);
        }

        protected override void InternalProcessMessage(IOCPReaderWriter Context, byte[] Message)
        {
            base.InternalProcessMessage(Context, Message);
            var call = new IOCPRPCCall();
            //XmlSerializer ser = new XmlSerializer(call.GetType());
            var buf = Message; //ASCIIEncoding.ASCII.GetBytes(Message);
            var ms = new MemoryStream();
            ms.Write(buf, 0, buf.Length);
            ms.Position = 0;
            //call = (IOCPRPCCall) ser.Deserialize(ms);
            call = (IOCPRPCCall)DeSerialize(ms, call.GetType());
            var rpcContext = new RpcContext(Context.Socket, Context, this, call);

            switch ((ResponseType)call.Response)
            {
                default:
                    {
                        _responseQ.Add(call);
                        return;
                    }
                    break;
                case ResponseType.Message:
                    {
                        if (OnMessage != null)
                        {
                            OnMessage(this, rpcContext, call.Params[0].ToString());
                        }
                        return;
                    }
                    break;
                case ResponseType.None:
                    {
                    }
                    break;
            }
            /*            if (call.Response != (int)ResponseType.None)
                        {
                            _responseQ.Add(call);
                            return;
                        }*/

            MethodHandlerContext ctx = null;
            {
                var session = _sessions.GetSession();
                try
                {


                    if (session.TryGetValue(call.Method, out ctx))
                    {
                        try
                        {
                            // THIS MIGHT BE NESSESARY FOR MULTIPLE THREADS
                            object[] outp;
                            lock (ctx.Instance)
                            {

                                var inputParams = new object[call.Params.Count + 1];
                                inputParams[0] = rpcContext;
                                for (int i = 1; i <= call.Params.Count; i++)
                                {
                                    inputParams[i] = call.Params[i - 1];
                                }
                                outp = (object[]) ctx.MethodInfo.Invoke(ctx.Instance, inputParams);
                                call.SetResponse(ResponseType.Success, "");
                                call.Params.Clear();
                                foreach (object p in outp)
                                {
                                    call.Params.Add(p);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (e is TargetInvocationException)
                                e = ((TargetInvocationException) e).InnerException;
                            call.SetException(e);
                        }
                    }
                    else
                    {
                        call.SetResponse(ResponseType.Error, string.Format("Method not found: {0}", call.Method));
                    }
                }
                finally
                {
                    _sessions.ReleaseSession(session);
                }
            }
            Execute(Context, call);

            /* foreach (var handler in _handlers)
             {
                
                 foreach (var metho in handler.GetType().GetMethods())
                 {
                     if (metho.Name == call.Method)
                     {
                         object[] outp = (object[])metho.Invoke(handler, call.Params.ToArray());
                         call.Params.Clear();
                         foreach (object p in outp)
                         {
                             call.Params.Add(p);
                         }
                         call.IsResponse = true;
                         Execute(Context, call);
                     }
                 }
             }*/
        }
    }
    internal class MethodHandlerContext
    {
        private MethodInfo _info;
        public MethodInfo MethodInfo
        {
            get
            {
                return _info;
            }
        }

        private object _instance;
        public object Instance
        {
            get
            {
                return _instance;
            }
        }
        public MethodHandlerContext(object Instance, MethodInfo Info)
        {
            _instance = Instance;
            _info = Info;
        }
    }
    public interface IRpcCall
    {
        List<object> Params { get; }
        string Method { get; }
    }
    [Serializable]
    public class IOCPRPCCall : IRpcCall
    {
        private List<object> _Params;
        internal void SetException(Exception e)
        {
            Params.Clear();
            Params.Add(e.Message);
            Params.Add(e.StackTrace);
            this.Response = (int) ResponseType.Error;
        }

        internal void SetResponse(ResponseType Response, string Message)
        {
            Params.Clear();
            Params.Add(Message);
            this.Response = (int)Response;            
        }

        public IOCPRPCCall()
        {
            _Params = new List<object>();
            _internalId++;
            Id = _internalId;
            Response = (int) ResponseType.None;
        }
        public string Method { get; set; }
        public int Response { get; set; }
        public static Int64 _internalId = 0;
        public Int64 Id { get; set; }

        public List<object> Params
        {
            get
            {
                return _Params;

            }
            set
            {
                _Params = value;
            }
        }
    }
    enum ResponseType
    {
        Unknown = 0,
        None = 1,
        Success = 2,
        Error = 3,
        Message = 4
    }

    internal class SessionList : List<Session>
    {
        private Semaphore _lock;
        public SessionList()
        {
            
        }
        public void InitSessions()
        {
            _lock = new Semaphore(Count, Count);
        }
        public Session GetSession()
        {
            if (_lock.WaitOne())
            {
                lock (this)
                {
                    foreach (var session in this)
                    {
                        if (!session.InSession)
                        {
                            session.InSession = true;
                            return session;
                        }
                    }
                }
            }
            throw new Exception("Could not get session");
        }
        public void ReleaseSession(Session Session)
        {
            lock (this)
            {
                Session.InSession = false;
            }
            _lock.Release();
        }
    }

    public interface ISession
    {        
    }

    internal class Session : Dictionary<string, MethodHandlerContext>, ISession
    {
        internal bool InSession = false;
    }
    public class RpcHandler : IRpcHandler
    {
        public object GetInstance()
        {
            return this;
        }
    }

    internal class RpcContext : IRpcContext
    {
        private Socket _Socket;
        private IOCPReaderWriter _IO;
        private IOCPMessageQueueRPC _MessageQueue;
        private IRpcCall _Call;
        public RpcContext(Socket Socket, IOCPReaderWriter IO, IOCPMessageQueueRPC MessageQueue, IRpcCall Call)
        {
            _Socket = Socket;
            _IO = IO;
            _MessageQueue = MessageQueue;
            _Call = Call;
        }
        public Socket Socket
        {
            get { return _Socket; }
        }

        public IOCPReaderWriter IO
        {
            get { return _IO; }
        }

        public IOCPMessageQueueRPC MessageQueue
        {
            get { return _MessageQueue; }
        }

        public IRpcCall Call
        {
            get { return _Call; }
        }
    }

    [Serializable]
    public class Param
    {
        public string Name;
        public object Value;
    }
}
