using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ACSR.Core.Strings;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ACSR.Core.Processes
{
    public delegate void ProcessMessageEvent(string AMessage);

    public class ProcessFactory
    {

        Process CreateProcess(string fileName, string arguments, string workingDirectory = null)
        {
            if (workingDirectory == null)
            {
                workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            var compiler = new Process();
            compiler.StartInfo.FileName = fileName;
            compiler.StartInfo.Arguments = arguments;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.StandardOutputEncoding = Encoding.ASCII;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.StartInfo.WorkingDirectory = workingDirectory;
            return compiler;
        }
        public ProcessContext CreateProcessContext(string fileName, string arguments, string workingDirectory = null, int WaitFor = 0)
        {
            return new ProcessContext(this, CreateProcess(fileName, arguments, workingDirectory), WaitFor);
        }

        ProcessContext CreateFromArgs(IEnumerable<string> args)
        {
            var cmd = this.CMDLineHelper;
            cmd.Parse(args);
            

            int waitFor = ((cmd.HasSwitch("waitTime") && (!string.IsNullOrEmpty(cmd.ParamAfterSwitch("t")))))
                                ? Int32.Parse(cmd.ParamAfterSwitch("t"))
                                : 0;
            //bool doWait = cmd.HasSwitch("waitTime");
            bool doKillOnTimeout = cmd.HasSwitch("killOnTimeOut");
            string prms = cmd.ParamsAfterSwitchAsString("p", true);

            var procCtx = CreateProcessContext(cmd.Args[0], prms);
            procCtx.WaitFor = waitFor;
            procCtx.KillOnTimeOut = doKillOnTimeout;
                
            
            return procCtx;
        }
        ProcessContext CreateFromString(string command)
        {
            return CreateFromArgs(ParseArgs(command));
        }

        List<string> ParseArgs(string command)
        {
            var s = new StringSplitter();
            s.Split_test(command);
            var args = new List<string>();
            foreach (string t in s.Lines)
            {
                args.Add(t);
            }
            return args;
        }

        private CmdLineHelper _CMDLineHelper;
        public CmdLineHelper CMDLineHelper
        {
            get
            {
                if (_CMDLineHelper == null)
                {
                    _CMDLineHelper = new CmdLineHelper();
                }
                return _CMDLineHelper;
            }
            set
            {
                _CMDLineHelper = value;
            }
        }
    }


    public class ProcessContext
    {
        public event ProcessMessageEvent OnMessage;
        public event ProcessMessageEvent OnError;

        public Process Process { get; set; }
        public int WaitFor { get; set; }

        public bool KillOnTimeOut { get; set; }
        ProcessFactory _factory;
        public ProcessContext(
            ProcessFactory factory,
            Process process, int waitFor = 0, bool killOnTimeOut = false)
        {
            this.KillOnTimeOut = killOnTimeOut;
            this.Process = process;
            this.WaitFor = waitFor;
            this._factory = factory;

            _standardOutput = new ConsoleOutput();
            _errorOutput = new ConsoleOutput();

        }

        public ProcessContext CreateSpawn(string fileName, string arguments, string workingDirectory = null)
        {
            var newCtx = _factory.CreateProcessContext(fileName, arguments, workingDirectory);
            newCtx.OnMessage +=new ProcessMessageEvent(newCtx_OnMessage);
            newCtx.OnError +=new ProcessMessageEvent(newCtx_OnError);
            return newCtx;
        }

        void newCtx_OnError(string AMessage)
        {
            LogError(AMessage);
        }

        void newCtx_OnMessage(string AMessage)
        {
            LogMessage(AMessage);
        }

        public void LogMessage(string AMessage)
        {
            if (OnMessage != null)
                OnMessage(AMessage);
        }
        public void LogError(string AMessage)
        {
            if (OnError != null)
                OnError(AMessage);
        }
        AutoResetEvent _event;

        ConsoleOutput _standardOutput;

        public ConsoleOutput StandardOutput
        {
            get { return _standardOutput; }
        }
        ConsoleOutput _errorOutput;

        public ConsoleOutput ErrorOutput
        {
            get { return _errorOutput; }
        }

        public void Start()
        {
            
            byte[] buffer;
            buffer = new byte[1024 * 64];
            _event = new AutoResetEvent(false);
            Process.Start();
            Process.StandardOutput.BaseStream.BeginRead(buffer, 0, buffer.Length,
                new AsyncCallback(ReadEvent), new StreamReadContext(Process.StandardOutput.BaseStream, 
                    buffer, MessageType.Message, _standardOutput));
            buffer = new byte[1024 * 64];
            Process.StandardError.BaseStream.BeginRead(buffer, 0, buffer.Length,
                new AsyncCallback(ReadEvent), new StreamReadContext(Process.StandardError.BaseStream,
                    buffer, MessageType.Error, _errorOutput));
            
            //Process.Exited += new EventHandler(Process_Exited);
            if (WaitFor >= 0)
            {
                if (WaitFor == 0)
                {
                    _event.WaitOne();
                    Process.WaitForExit();
                }
                else
                {
                    if (!Process.WaitForExit(WaitFor))
                    {
                        LogError(string.Format("Process ({0}) Timed out.", Process.StartInfo.Arguments));
                        if (KillOnTimeOut)
                        {
                            Process.Kill();
                            LogError(string.Format("Process ({0}) Terminated.", Process.StartInfo.Arguments));
                        }
                    }
                }
            }
        }

        void Process_Exited(object sender, EventArgs e)
        {
           // _event.Reset();
        }
        void ReadEvent(IAsyncResult result)
        {
            var ctx = (StreamReadContext)result.AsyncState;
            int amountRead = ctx.Stream.EndRead(result);
            if (amountRead > 0)
            {
                var text = Encoding.ASCII.GetString(ctx.Buffer, 0, amountRead);
                LogMessage(text);
                ctx.ConsoleOutput.WriteString(text);
                ctx.Stream.BeginRead(ctx.Buffer, 0, ctx.Buffer.Length, ReadEvent, ctx);
            }
            else
            {
                if (ctx.MessageType == MessageType.Message)
                {
                    _event.Set();
                }
            }
           
        }
    }
    enum MessageType
    {
        Unknown = 0,
        Message = 1,
        Error = 2
    }
    class StreamReadContext
    {
        Stream _stream;
        byte[] _buffer;
        MessageType _messageType;

        internal MessageType MessageType
        {
            get { return _messageType; }
            set { _messageType = value; }
        }
        public Stream Stream
        {
            get
            {
                return _stream;
            }
        }
        public byte[] Buffer
        {
            get
            {
                return _buffer;
            }
        }
        ConsoleOutput _consoleOutput;

        public ConsoleOutput ConsoleOutput
        {
            get { return _consoleOutput; }
            set { _consoleOutput = value; }
        }
        public StreamReadContext(Stream stream, byte[] buffer, MessageType messageType, 
            ConsoleOutput consoleOutput)
        {
            this._stream = stream;
            this._buffer = buffer;
            this._messageType = messageType;
            this._consoleOutput = consoleOutput;
        }
    }
    public class ConsoleOutput
    {
        StringBuilder _builder;
        public void Clear()
        {
            _builder = new StringBuilder();
        }
        public ConsoleOutput()
        {
            Clear();
        }
        public void WriteString(string s)
        {
            _builder.Append(s);
        }
        public string GetValue()
        {
            return _builder.ToString();
        }
    }
}
