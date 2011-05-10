using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Core.Networking.IOCP
{
    public class IOCPSocket
    {
        public event SocketIOEvent OnDisconnect;
        public event SocketIOEvent OnConnect;
        public void InitSocketIO(IOCPReaderWriter IO)
        {
            IO.OnDisconnect += new SocketIOEvent(IO_OnDisconnect);
        }

        protected void RaiseEventConnect(IIOCPSocketContext context)
        {
            if (OnConnect != null)
                OnConnect(this, context);
            
        }

        void IO_OnDisconnect(object sender, IIOCPSocketContext context)
        {
            if (OnDisconnect != null)
                OnDisconnect(this, context);
        }
    }
}
