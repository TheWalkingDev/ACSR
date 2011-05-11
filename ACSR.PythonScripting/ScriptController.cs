using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using System.Collections;
using System.Reflection;

namespace ACSR.PythonScripting
{
    public delegate void FlushBufferEvent(byte[] Data);
    public delegate void MessageEvent(object sender, string Message);
    public class ScriptController
    {
        

        public event MessageEvent OnMessage;

        private ScriptRuntimeSetup _setup;
        private ScriptRuntime _runtime;
        private ScriptEngine _engine;
        protected ScriptScope _scope;
        public ScriptScope Scope
        {
            get
            {
                return _scope;
            }
        }
        public ScriptEngine Engine
        {
            get
            {
                return _engine;
            }
        }

        private BufferedStream _outputStream;

        public ScriptController(bool ADebug)
        {
            // _SearchPaths = new List<string>();
            
            _setup = new ScriptRuntimeSetup();
            _setup.DebugMode = true;
            _setup.LanguageSetups.Add(Python.CreateLanguageSetup(null));
            _setup.DebugMode = ADebug;
            
            _runtime = new ScriptRuntime(_setup);
            _engine = _runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);
            var bufOut = new BufferedStream(256, 100);
            _outputStream = bufOut;
            bufOut.OnFlushBuffer +=new FlushBufferEvent(bufOut_OnFlushBuffer);
            _engine.Runtime.IO.SetOutput(bufOut, ASCIIEncoding.ASCII);
            _engine.Runtime.IO.SetErrorOutput(bufOut, ASCIIEncoding.ASCII);

            SetSearchPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        public void SetSearchPath(string path)
        {
            var sp = _engine.GetSearchPaths();
            foreach (var p in sp)
            {
                if (string.Compare(p, path, false) == 0)
                {
                    return;
                }
            }
            sp.Add(path);
            _engine.SetSearchPaths(sp);
        }

        public void SetSysArgV<T> (IEnumerable<T> args)
        {
            var a = new List<T>();
            foreach (var arg in args)
            {
                a.Add(arg);
            }
            _engine.GetSysModule().SetVariable("argv", a);


        }
        ScriptSource getScriptSourceFromString(string Script)
        {
            _engine.SetSearchPaths(SearchPaths);
            return _engine.CreateScriptSourceFromString(Script);
        }

        public IScriptContext CreateScriptContext()
        {
            var ctx = new ScriptContext(_engine.CreateScope(), null,
                _outputStream, _engine);
            return ctx;
        }

        public IScriptContext CreateScriptContextFromString(string Script)
        {
            var script = getScriptSourceFromString(Script);
            var ctx = CreateScriptContext();
            ctx.Script = script;
            return CreateScriptContextFromString(Script, ctx);
        }

        public IScriptContext CreateScriptContextFromString(string script, IScriptContext existingContext)
        {
            
            if (existingContext == null)
            {
                return CreateScriptContextFromString(script);
            }
            var returnScript = getScriptSourceFromString(script);
            existingContext.Script = returnScript;
            return existingContext;
            //return new ScriptContext(Ctx.GetScriptScope(), script, _outputStream);
        }

        public IScriptContext CreateScriptContextFromFile(string FileName)
        {
            // we could use usefile here.
            _engine.SetSearchPaths(SearchPaths);
            var script = _engine.CreateScriptSourceFromFile(FileName);
            var scope = _engine.CreateScope();
            return new ScriptContext(scope, script, _outputStream, _engine);
        }

        void bufOut_OnFlushBuffer(byte[] Data)
        {
            if (Data.Length == 0)
                return;
            if (OnMessage != null)
                OnMessage(this, ASCIIEncoding.ASCII.GetString(Data));
        }
     
        //private ICollection<string> _SearchPaths;
        public ICollection<string> SearchPaths
        {
            get
            {
                return _engine.GetSearchPaths();
            }
            set
            {
                _engine.SetSearchPaths(value);
            }
        }

    }

    public class LocalisedScope : ACSR.PythonScripting.ILocalisedScope
    {
        ScriptContext _scriptContext;
        ScriptScope _scope;
        public LocalisedScope(ScriptContext scriptContext, ScriptScope scope)
        {
            _scriptContext = scriptContext;
            _scope = scope;
        }
        public ILocalisedScope SetVariable(string name, object value)
        {
            _scope.SetVariable(name, value);
            return this;
        }
        public dynamic Evaluate(string expression)
        {
            return _scriptContext._engine.Execute(expression, _scope);
        }
    }

    public class ScriptContext : IScriptContext
    {
        private ScriptScope _scope;
        private ScriptSource _script;
        private BufferedStream _outputBuffer;
        internal ScriptEngine _engine;

        public ILocalisedScope CreateLocalScope()
        {
            return new LocalisedScope(this, _engine.CreateScope());
        }

        public dynamic GetGlobals()
        {
            var tempScope = CreateLocalScope();
            tempScope.SetVariable("g", _engine.Execute("globals().items()", _scope));
            return tempScope.Evaluate("g.sort()\ng");
 
        }
        public void SetGlobals(object Globals)
        {            
            Python.GetBuiltinModule(_engine).SetVariable("globals", Globals);
        }

      

        public dynamic ExecuteString(string script)
        {
            return _engine.Execute(script, _scope);
        }

        public dynamic Execute()
        {         
            var result = _script.Execute(_scope);
            _outputBuffer.FlushBuffer();
            return result;
        }

        public void SetVariable(string Name, object Value)
        {
            _scope.SetVariable(Name, Value);
        }

        public void InjectMembers(object source)
        {
            foreach (var member in source.GetType().GetMembers())
            {

            }
        }

        public ScriptScope GetScriptScope()
        {
            return _scope;
        }
        

        public dynamic Scope
        {
            get
            {
                return _scope;
            }
        }
        public ScriptSource Script
        {
            get
            {
                return _script;
            }
            set
            {
                _script = value;
            }
        }

        public ScriptContext(ScriptScope Scope, ScriptSource Script, 
            BufferedStream outputBuffer,
            ScriptEngine Engine)
        {
            this._engine = Engine;
            this._outputBuffer = outputBuffer;
            this._scope = Scope;
            this._script = Script;
          

        }

        ~ScriptContext()
        {
            //_outputBuffer.FlushBuffer();
        }

        #region IScriptContext Members


        public void FlushBuffer()
        {
            _outputBuffer.FlushBuffer();
        }

        #endregion
    }


}
