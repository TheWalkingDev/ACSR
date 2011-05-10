using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ACSR.Core.System.Threading
{
    public delegate void FastThreadMethod(Thread AThread, params object[] args);
    public static class ThreadUtils
    {

        public static Thread FastThread(FastThreadMethod method, params object[] args)
        {
           return FastThread(method, null, args);
        }
        public static Thread FastThread(FastThreadMethod method, Thread AThread, params object[] args)
        {
           return new ThreadWrapper(method, AThread, args).TheThread;
        }

    }
    class ThreadWrapper
    {
        private Thread _thread;
        public Thread TheThread
        {
            get
            {
                if (_thread == null)
                {
                    _thread = new Thread(Run);    
                }
                return _thread;
            }
        }
        private void Run()
        {
            _method(this.TheThread, _params);
        }

        private FastThreadMethod _method;
        private object[] _params;
        public ThreadWrapper(FastThreadMethod method, params object[] args)
            : this(method, null, args)
        {
            
        }
        public ThreadWrapper(FastThreadMethod method, Thread AThread, params object[] args)
        {
            _params = args;
            _method = method;
            _thread = AThread;
        }
    }
}
