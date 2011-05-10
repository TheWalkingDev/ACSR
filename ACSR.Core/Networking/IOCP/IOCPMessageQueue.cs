using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ACSR.Core.System.Threading;

namespace ACSR.Core.Networking.IOCP
{
    public class IOCPMessageQueue : IDisposable
    {
        private List<MessageContext> _MessageQueue;
        private AutoResetEvent _responseEvent;
        private int _threads;
        private bool _Terminated = false;

        void Worker(Thread AThread, params object[] args)
        {
            do
            {
                ProcessMessages(1000);
            } while (!_Terminated);
            if (Interlocked.Decrement(ref _threads) == 0)
            {
                _responseEvent.Set();
            }
        }
        public void StartServer(int Threads)
        {
            _Terminated = false;
            for (int i = 1; i <= Threads; i++)
            {
                Interlocked.Increment(ref _threads);
                ThreadUtils.FastThread(Worker).Start();
            }
        }
        public IOCPMessageQueue()
        {
            _MessageQueue = new List<MessageContext>();
            _responseEvent = new AutoResetEvent(false);
        }
        public void Put(IOCPReaderWriter Context, byte[] Message)
        {
            lock (_MessageQueue)
            {
                /*var buffer = new Byte[ctx.Buffer.Length];
                Buffer.BlockCopy(ctx.Buffer, 0, buffer, 0, buffer.Length);*/
                _MessageQueue.Add(new MessageContext(Context, Message));
            }
            _responseEvent.Set();

        }

        protected virtual void InternalProcessMessage(IOCPReaderWriter Context, byte[] Message)
        {
          
        }
        public void ProcessMessages(int Timeout)
        {
            MessageContext Message = null;
            do
            {
                Message = null;
                if (_responseEvent.WaitOne(Timeout))
                {
                    lock (_MessageQueue)
                    {
                        if (_MessageQueue.Count > 0)
                        {
                            Message = _MessageQueue[_MessageQueue.Count - 1];
                            _MessageQueue.RemoveAt(_MessageQueue.Count - 1);
                        }
                    }
                    if (Message == null)
                        return;
                    InternalProcessMessage(Message.Reader, Message.Message);
                    
                }

            } while (Message != null);
            
            //for (int i = 0; i < _MessageQueue.Count; i++)
        }
        public void StopServer()
        {
            _Terminated = true;
            do
            {
                _responseEvent.WaitOne(1000);
            } while (_threads > 0);
            
        }

        public void Dispose()
        {
            StopServer();
        }
    }
    internal class MessageContext
    {
        internal IOCPReaderWriter Reader { get; set; }
        internal byte[] Message;
        public MessageContext(IOCPReaderWriter Reader, byte[] Message)
        {
            this.Reader = Reader;
            this.Message = Message;
            
        }
    }
    
}
