using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.PythonScripting
{
    public class CLRReference
    {
        Func<object> _getter;
        Action<object> _setter;

        public CLRReference(Func<object> Getter, Action<object> Setter)
        {
            _getter = Getter;
            _setter = Setter;

        }

        public object Value
        {
            get
            {
                return _getter();
            }
            set
            {                
                _setter(value);
            }
        }
    }
}
