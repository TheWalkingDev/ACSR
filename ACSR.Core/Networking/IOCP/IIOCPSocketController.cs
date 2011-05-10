using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    public interface IIOCPSocketController
    {
        void DoDisconnectSocket();
    }
}
