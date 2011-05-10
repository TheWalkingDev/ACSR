using System;
namespace ACSR.PythonScripting
{
    public interface ILocalisedScope
    {
        dynamic Evaluate(string expression);
        ILocalisedScope SetVariable(string name, object value);
    }
}
