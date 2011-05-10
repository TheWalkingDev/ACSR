using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace ACSR.Core.Networking.IOCP
{
    public class IOCPRPC : IOCPReaderWriter
    {
      

        public IOCPRPC(Socket Socket) : base(Socket)
        {
           
        }
        protected override void InternalMessageComplete(byte[] data)
        {
            if (MessageQueue == null || !MessageQueue.GetType().IsSubclassOf(typeof(IOCPMessageQueueRPC)))
            {
                throw new Exception("MessageQueue must be assigned and of type IOCPMessageQueueRPC");
            }
            MessageQueue.Put(this, data);
            
        }
    }
   

}
