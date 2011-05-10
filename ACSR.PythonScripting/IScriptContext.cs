using System;
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;
namespace ACSR.PythonScripting
{
    public interface IScriptContext
    {
        ScriptScope GetScriptScope();
        void SetVariable(string Name, object Value);
        dynamic Execute();
        dynamic ExecuteString(string script);
        void InjectMembers(object source);
        dynamic Scope { get;  }
        ScriptSource Script { get; set; }
        void SetGlobals(object Globals);
        dynamic GetGlobals();
        ILocalisedScope CreateLocalScope();
        void FlushBuffer();
    }
}
