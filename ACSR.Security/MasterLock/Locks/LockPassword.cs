using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Security.MasterLock.Locks
{
    public class LockPassword : BaseLock
    {
        public LockPassword(MasterLock Master, string Name)
            : base(Master, Name)
        {           
        }
        protected override string GetLockName()
        {
            return "LockPassword";
        }
        public override string GetKey()
        {
            return "mypassword";
        }
    }
}
