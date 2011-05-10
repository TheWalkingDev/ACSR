using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACSR.Core.Networking.IOCP;
using ACSR.Core.Networking.IOCP.RPCServices;
using ACSR.Core.Processes;

namespace ACSR.PythonScripting.ClientServer
{
    public class IronPythonServer
    {
        public delegate void MessageEvent(object sender, IRpcContext context, string Message, IPythonServerContext serverContext);

        public event MessageEvent OnMessage;
        private IOCPServer _server;
        public IOCPServer Server
        {
            get
            {
                return _server;
            }
        }

        private IOCPMessageQueueRPC _q;
        public IOCPMessageQueueRPC MessageQueue
        {
            get
            {
                return _q;
            }
        }
        private int _port;
        public IronPythonServer(int Port)
        {
            _port = Port;
            _q = new IOCPMessageQueueRPC();
            _server = new IOCPServer(_port);
            _server.MessageQueue = _q;
            foreach (var session in _q.RegisterRPCHandlers())
            {
                _q.RegisterRPCHandler(session, new IronPythonHandler(this));
            }            
        }
        public void Start()
        {
            _server.Start();
            _q.StartServer(10);
        }
        public void Stop()
        {
            _server.Stop();
            _q.StopServer();            
            
        }
        internal void HandleMessage(IRpcContext context, string Message, IPythonServerContext serverContext)
        {
            if (OnMessage != null)
            {
                OnMessage(this, context, Message, serverContext);
            }
        }      
    }
    public interface IPythonServerContext
    {
        string FileName { get; }
        IEnumerable<string> Arguments { get; }
    }
    internal class IronPythonHandler : RpcHandler, IPythonServerContext, IRpcHandler 
    {
        private IRpcContext _context;
        private IronPythonServer _server;
        private string _fileName;
        private string _scriptFile;
        private string _code;
        private IEnumerable<string> _args;
        private ScriptController _controller;
        private string _processName;
        private CmdLineHelper _cmd;
        private static Dictionary<string, ScriptController> _processControllers = new Dictionary<string, ScriptController>();
        
        public IronPythonHandler(IronPythonServer server)
        {
            _server = server;
        }        
        private ScriptController CreateController()
        {
            _controller = new ScriptController(false);
          //  _controller.Engine.Runtime.LoadAssembly(typeof(System.Diagnostics.Process).Assembly);
            _controller.OnMessage += new MessageEvent(_controller_OnMessage);
            return _controller;
            
        }

        void _controller_OnMessage(object sender, string Message)
        {
            try
            {
                var c = new IOCPServerClients();
                _server.Server.GetClients(c);
                _server.MessageQueue.BroadCastMessage(c, Message);

                if (_context.IO.Socket.Connected)
                {
                    _server.HandleMessage(_context, Message, this);

                }
                else
                {
                    _server.HandleMessage(_context, Message, this);
                }
            }
            catch
            {

            }
        }

        private void GetParams(IEnumerable<string> args)
        {
            _cmd = new CmdLineHelper();
            _cmd.Parse(args);
            _processName = _cmd.ParamAfterSwitch("ProcessName");
            _scriptFile = _cmd.ParamAfterSwitch("ScriptFile");
            _code = _cmd.ParamAfterSwitch("Code");
        }
        private ScriptController BeginProcess(string ProcessName)
        {
            lock (_processControllers)
            {
                if (!_processControllers.TryGetValue(ProcessName, out _controller))
                {
                    _controller = CreateController();
                    _processControllers[ProcessName] = _controller;
                }
                return _controller;
            }
        }

        public object[] RunScript(IRpcContext context, string FileName, List<string> Args)
        {
            GetParams(Args);
            var args = _cmd.ParamsAfterSwitch("args");

            _context = context;
            _fileName = FileName;
            _args = args;
            _controller = BeginProcess(_processName);

            //c.SearchPaths.Add(@"E:\Dev\Source\AndresIPyResource\Scripts");
            var ctx = _controller.CreateScriptContextFromFile(FileName);
            _controller.SetSysArgV(args);

            ctx.Execute();
            return new object[0];
        }
        public object[] Eval(IRpcContext context, List<string> Args)
        {
            GetParams(Args);
            _controller = BeginProcess(_processName);
            var ctx = _controller.CreateScriptContextFromString(_code);

            ctx.Execute();

            //code.Run(_controller.Scope);
            return new object[0];            
        }
      

        public string FileName
        {
            get { return _fileName; }
        }

        public IEnumerable<string> Arguments
        {
            get { return _args; }
        }

        
    }
}
