using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ACSR.Core.Networking.IOCP;

namespace ACSR.PythonScripting.ClientServer
{
    public class IronPythonClient
    {
        private string _host;
        private int _port;
        private IOCPMessageQueueRPC _q;
        private IOCPClient _client;
        public event IOCPMessageQueueRPC.MessageEvent OnMessage;
        public IronPythonClient(string Host, int Port)
        {
            _host = Host;
            _port = Port;
            _q = new IOCPMessageQueueRPC();
            _q.OnMessage += new IOCPMessageQueueRPC.MessageEvent(_q_OnMessage);
            _client = new IOCPClient();
            _client.MessageQueue = _q;
        }
        public void Connect()
        {
            _client.Connect(_host, _port);
        }
        
        public void AddArg(List<string> args, string Name, string Value)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                
                args.Add(Name);
                args.Add(Value);
                
            }
        }

        public void Run<T>(string ScriptFile, string ProcessName, IEnumerable<T> Args)
        {
            var args = new List<string>();
            
            AddArg(args, "-ScriptFile", ScriptFile);
            AddArg(args, "-ProcessName", ProcessName);

            if (args.Count > 0)
            {
                args.Add("-args");
                foreach (var arg in Args)
                {
                    args.Add(arg.ToString());
                }
            }
            _q.Execute(_client.ReaderWriter, "ACSR.PythonScripting.ClientServer.IronPythonHandler.RunScript", ScriptFile, args);
        }

        public void Eval(string Code, string ProcessName)
        {
            var args = new List<string>();

            AddArg(args, "-Code", Code);
            AddArg(args, "-ProcessName", ProcessName);
            _q.Execute(_client.ReaderWriter, "ACSR.PythonScripting.ClientServer.IronPythonHandler.Eval", args);
        }

        void _q_OnMessage(object sender, IRpcContext context, string message)
        {
            if (OnMessage != null)
            {
                OnMessage(sender, context, message);
            }
        }
    }
}
