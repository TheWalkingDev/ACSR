using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace ACSR.PythonScripting
{
    public delegate void ScriptMonitorEvent(object sender, ScriptMonitorEventArgs e);
    public class ScriptMonitor
    {
        FileSystemWatcher _watcher;
        string _fullPath;
        IScriptContext _context;
        DateTime _lastLoad = DateTime.MinValue;
        string _lastHash = null;

        public event ScriptMonitorEvent OnScriptLoading;
        public event ScriptMonitorEvent OnScriptUnLoading;
        public event ScriptMonitorEvent OnScriptLoaded;
        ScriptController _engine;

        public MessageEvent OnEngineMessage;

        static string GetMD5Hash(string pathName)
        {
           string strResult = "";
           string strHashData = "";

           byte[] arrbytHashValue;
          // System.IO.FileStream oFileStream = null;

           System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher=
                      new System.Security.Cryptography.MD5CryptoServiceProvider();


           using (var oFileStream = new FileStream(pathName, FileMode.Open))
           {
               //oFileStream = GetFileStream(pathName);
               arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);
               oFileStream.Close();
           }

            strHashData = System.BitConverter.ToString(arrbytHashValue);
           // strHashData = strHashData.Replace("-", "");
            strResult = strHashData;
         
            return(strResult);
        }
        string GetMD5Hash()
        {
            return GetMD5Hash(_fullPath);
        }

        bool FileWasReallyModified()
        {
            return (_lastLoad == null || getLastModifiedTime() > _lastLoad || _lastHash == null || _lastHash.CompareTo(GetMD5Hash()) != 0);
        }
        void FlagFileModified()
        {
            _lastLoad = getLastModifiedTime();
            _lastHash = GetMD5Hash();
        }

        DateTime getLastModifiedTime()
        {
            FileInfo fi = new FileInfo(_fullPath);
            return fi.LastWriteTime;
        }
        void CreateScript()
        {
            _engine = new ScriptController(true);
            _context = _engine.CreateScriptContextFromFile(_fullPath);
            _engine.OnMessage += new MessageEvent(_engine_OnMessage);
            
        }

        void _engine_OnMessage(object sender, string Message)
        {
            if (OnEngineMessage != null)
            {
                OnEngineMessage(sender, Message);
            }
        }
        int _loadCount = 0;

        void ReloadScript()
        {
            int count = Interlocked.Increment(ref _loadCount);
            try
            {
                if (count == 1)
                {
                    FlagFileModified();
                    if (_context != null)
                    {
                        RaiseScriptMonitorEvent(OnScriptUnLoading);
                    }
                    CreateScript();
                    RaiseScriptMonitorEvent(OnScriptLoading);

                    _context.Execute();
                    RaiseScriptMonitorEvent(OnScriptLoaded);
                    
                }
            }
            finally
            {
                Interlocked.Decrement(ref _loadCount);
            }
        }
        public ScriptMonitor(string fileName)
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = Path.GetDirectoryName(fileName);
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            _fullPath = fileName;
        }

        public void Activate()
        {
            ReloadScript();
            _watcher.EnableRaisingEvents = true;
        }
        public void DeActivate()
        {
            _watcher.EnableRaisingEvents = true;
        }

        void RaiseScriptMonitorEvent(ScriptMonitorEvent ev)
        {
            if (ev != null)
            {
                ev(_context, new ScriptMonitorEventArgs(_context, this));
            }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.CompareTo(_fullPath) == 0)
            {
                if (FileWasReallyModified())
                {
                    ReloadScript();
                }
            }
        }
        ~ScriptMonitor()
        {
            DeActivate();
        }
    }
    public class ScriptMonitorEventArgs : EventArgs
    {
        public IScriptContext Context { get; set; }
        public ScriptMonitor Monitor { get; set; }
        public ScriptMonitorEventArgs(IScriptContext Context, ScriptMonitor Monitor)
        {
            this.Context = Context;
            this.Monitor = Monitor;
        }
    }
}
