using System;
using System.Collections.Generic;
namespace ACSR.Core.Processes
{
    public interface ICommandParameters
    {
        List<string> Args { get; }
        bool HasSwitch(string ASwtich);
        string ParamAfterSwitch(string ASwitch);
        IEnumerable<string> ParamsAfterSwitch(string ASwitch);
        string ParamsAfterSwitchAsString(string ASwitch, bool AAddQuotes);
        void Parse(IEnumerable<string> AArgs);
        void ParseString(string args);
    }
}
