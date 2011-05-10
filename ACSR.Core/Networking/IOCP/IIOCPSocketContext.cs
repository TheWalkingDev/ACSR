using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    public interface IIOCPSocketContext
    {
        Socket Socket
        { get;
        }
        IOCPReaderWriter IO { get; }
        
    }
}
