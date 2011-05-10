using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Controls.ThirdParty.Python
{
    public class PyInspector
    {
        Dictionary<string, object> _globals;
        public PyInspector()
        {
            _globals = new Dictionary<string, object>();
            
        }
        public PyInspector SetVariable(string Name, object Value)
        {
            _globals[Name] = Value;
            return this;
            
        }
        public void Inspect()
        {
            var f = new FrmPyPad();
            foreach (var de in _globals)
            {
                f.Control.SetVariable(de.Key, de.Value);
            }
            f.Control.UpdateVariables();
            f.ShowDialog();
        }

    }
}
