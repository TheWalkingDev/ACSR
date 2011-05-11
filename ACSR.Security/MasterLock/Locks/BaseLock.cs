using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ACSR.Core.Strings;

namespace ACSR.Security.MasterLock.Locks
{
    public class BaseLock
    {
        Dictionary<string, string> _Properties;
        MasterLock _Master;
        //ILockUI _UI = null;
        string _Name;
       /* protected ILockUI GetUI()
        {
            if (_UI == null)
            {
                _UI = _Master.GetUIFor(this.Name);
            }
            return _UI;
        }*/

        public BaseLock(MasterLock Master, string Name)
        {
            _Master = Master;
            _Name = Name;
        }

        public Dictionary<string, string> Properties 
        {
            get 
            {
                if (_Properties == null)
                {
                    _Properties = new Dictionary<string, string>();
                }
                return _Properties;
            }
        }
        public void LoadFromString(string s)
        {
            Properties.Clear();
            var ss = new StringReader(s);
            var count = ss.ReadInt();
            for (int i = 0; i < count; i++ )
            {
                string name = ss.ReadName();
                string value = ss.ReadName();
                Properties[name] = value;
            }
        }
        public string SaveToString()
        {
            var ss = new StringWriter();
            ss.WriteInt(Properties.Count);
            foreach (var p in Properties)
            {
                ss.WriteName(p.Key);
                ss.WriteName(p.Value);
            }
            return ss.ToString();
        }


        public string Name
        {
            get
            {
                return GetLockName();
            }
        }
        protected virtual string GetLockName()
        {
            return "BaseKey";

        }
        public virtual string GetKey()
        {
        //    if (GetUI() != null)
         //       return GetUI().GetKey(this);
            return "a very secure key";
        }
       /* public void Configure()
        {
            if (GetUI() != null)
                GetUI().Configure(this);
        }*/
    }

    public interface ILockConfiguration
    {
        string GetKey(string ConfigName, string Key);
    }
}
