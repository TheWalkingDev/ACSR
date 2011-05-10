using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Core.Networking.IOCP.RPCServices
{
    public interface IRpcHandler
    {
        Type GetType();
        object GetInstance();
        
    }
}
