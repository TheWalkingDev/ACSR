using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ACSR.Core.Processes
{
    class ProcessUtils
    {
        private Process[] _processes = null;
        public ProcessUtils() : this(true)
        {            
        }
        public ProcessUtils(bool AutoRefresh)
        {
             if (AutoRefresh)
             {
                 Refresh();
             }
        }
        public ProcessUtils(Process[] Processes)
        {
            _processes = Processes;
        }
        public void Refresh(Process[] processes)
        {
            _processes = processes;
        }
        public void Refresh()
        {
            Refresh( Process.GetProcesses());
        }

      
    }
}
