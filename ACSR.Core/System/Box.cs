using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Core.System
{
    public class Boxed<T>
    {
        public T Value;
        public Boxed()
            : this(default(T))
        {            
        }
        public Boxed(T AValue)
        {
            Value = AValue;
        }
    }

}
