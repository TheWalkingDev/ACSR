using System;
using System.Collections.Generic;
using System.Text;
using ACSR.Core.Strings;

namespace ACSR.Core.Processes
{
    public class CmdLineHelper : ACSR.Core.Processes.ICommandParameters
    {
        public string SwitchChars = "-/";
        private Dictionary<string, List<string>> _Switches;
        private List<string> _Args;
        public List<string> Args
        {
            get
            {
                return _Args;
            }
        }
        public CmdLineHelper() : this(true)
        {
        }

        public CmdLineHelper(bool ParseFromCmdLine)
        {
            _Switches = new Dictionary<string, List<string>>();
            _Args = new List<string>();
            if (ParseFromCmdLine)
            {
                Parse(Environment.GetCommandLineArgs());
            }
        }
        public string ParamAfterSwitch(string ASwitch)
        {
            List<string> param = null;
            if (_Switches.ContainsKey(ASwitch))
            {
                param = _Switches[ASwitch];
                if (param.Count > 0)
                    return param[0];
            }
            return null;
        }
        public IEnumerable<string> ParamsAfterSwitch(string ASwitch)
        {
            List<string> param = null;
            if (_Switches.ContainsKey(ASwitch))
            {
                param = _Switches[ASwitch];
                return param;
                
            }
            return null;            
        }
        public string ParamsAfterSwitchAsString (string ASwitch, bool AAddQuotes)
        {
            var prms = ParamsAfterSwitch(ASwitch);
            if (prms != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var p in prms)
                {
                    if (AAddQuotes)
                        sb.Append("\"");
                    sb.Append(p);
                    if (AAddQuotes)
                        sb.Append("\"");
                    sb.Append(" ");
                }
               
                return sb.ToString();
            }
            else
                return null;
        }
        public bool HasSwitch(string ASwtich)
        {
            return _Switches.ContainsKey(ASwtich);
        }

        bool IsCharNASwitch(string param, int index)
        {
            return (param.Length > index && SwitchChars.Contains(param[index].ToString()));
        }

        bool IsParamASwitch(string param)
        {
            return (IsCharNASwitch(param, 0));
        }

        public void ParseString(string args)
        {
            var s = new StringSplitter();
            s.Split_test(args);
            Parse(s.Lines);
        }

        public void Parse(IEnumerable<string> AArgs)
        {
            _Args.Clear();
            
            foreach (string arg in AArgs)
            {
                _Args.Add(arg);
            }
            int i = 0;
            List<string> LastSwitch = null;
            while (i < _Args.Count)
            {
                string arg = _Args[i];
                if (IsParamASwitch(arg))
                {
                    
                    char[] buf = new char[arg.Length - 1];
                    arg.CopyTo(1, buf, 0, buf.Length);
                    string sw = new string(buf);

                    if (IsCharNASwitch(sw, 0))
                    {
                        if (LastSwitch != null)
                        {
                            LastSwitch.Add(sw);
                        }
                    }
                    else
                    {

                        string param = null;
                        List<string> prms;
                        if (_Switches.ContainsKey(sw))
                        {
                            prms = _Switches[sw];
                        }
                        else
                        {
                            prms = new List<string>();
                            _Switches[sw] = prms;
                        }
                        LastSwitch = prms;
                    }
                }
                else
                {
                    if (LastSwitch != null)
                    {
                        LastSwitch.Add(arg);
                    }
                }
                i++;
            }
        }
    }
}