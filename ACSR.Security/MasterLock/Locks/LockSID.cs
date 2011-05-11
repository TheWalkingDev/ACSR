using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACSR.Security.MasterLock.Locks;

namespace ACSR.Security.MasterLock.Locks
{
    public class LockSID : BaseLock
    {

        public LockSID(MasterLock Master, string Name)
            : base(Master, Name)
        {           
        }
        public override string GetKey()
        {
            return "SID";
        }
        protected override string GetLockName()
        {
           return  "SID";
        }
    }
}
