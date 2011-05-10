using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    // Ok just started this concept, but not finished yet
    public class IOCPRPCProcess
    {
        private IOCPMessageQueueRPC _Queue;
        private IOCPReaderWriter _Context;
        public IOCPRPCProcess(IOCPMessageQueueRPC Queue, IOCPReaderWriter Context)
        {
            _Queue = Queue;
            _Context = Context;
        }
        public void BeginProcess(IEnumerable<IOCPProcessResults> Enumerator, string Name)
        {
            foreach (var result in Enumerator)
            {
               // _Queue.Execute(_Context, Name, Args);
            }
            /*string ProcessName, params object[] Args

            Queue.Execute(Context, ProcessName, Args);
            return null;*/
        }
    }
    public class IOCPRPCProcessHandler
    {
        private Dictionary<string, IEnumerable<IOCPProcessResults>> _results;
        private Dictionary<string, MethodContext> _Handlers;
        public void Register(object Handler)
        {
            foreach (MethodInfo m in Handler.GetType().GetMethods())
            {
                _Handlers[m.Name] = new MethodContext(m, Handler);
            }
        }
        public IOCPRPCProcessHandler()
        {
            _results = new Dictionary<string, IEnumerable<IOCPProcessResults>>();
            _Handlers = new Dictionary<string, MethodContext>();
        }
        public void HandleProcess(params object[] args)
        {
            IEnumerable<IOCPProcessResults> handler;
            string meth = (string) args[0];
            if (!_results.TryGetValue(meth, out handler))
            {
                object[] nArg = new object[args.Length-1];
                for (int i=1;i<args.Length-1;i++)
                {
                    nArg[i] = args[i];
                }

                 var h = _Handlers[meth];
                handler =  (IEnumerable<IOCPProcessResults>) h._methodInfo.Invoke(h._instance, nArg);
            }
            if (!handler.GetEnumerator().MoveNext())
            {
                _results[meth] = null;
            }
        }
    }
    internal class MethodContext
    {
        internal MethodInfo _methodInfo;
        internal object _instance;
        public MethodContext(MethodInfo MethodInfo, object Instance)
        {
            _methodInfo = MethodInfo;
            _instance = Instance;
        }
    }
    public class IOCPProcessResults : List<object>
    {
        
    }
}
