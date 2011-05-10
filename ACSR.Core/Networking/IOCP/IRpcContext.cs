using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    public interface IRpcContext : IIOCPSocketContext
    {
        
        IOCPMessageQueueRPC MessageQueue { get;  }
        IRpcCall Call { get; }
    }
}
