using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ACSR.Core.System.Threading;

namespace ACSR.Core.Disk
{

    public delegate void QueueEvent(object Sender, QueueEventArgs EventArgs);
    public class QueueEventArgs
    {
        public FileCopyEventArgs FileCopyEventArgs;
        public AdvFileCopy AdvFileCopy;
        public FileItem TheFileItem;
    }

    public enum QueueStatus
    {
        Unknown = 0,
        Waiting = 1,
        InProgress = 2,
        Completed = 3
    }

    public class FileItem
    {
        public QueueStatus Status;
        private string _source;
        private string _target;
        private QueueItem _queueItem;
        private long _bytesTransferred;
        public long BytesTransferred
        {
            get
            {
                return _bytesTransferred;
            }
        }
        internal void DoFileProgress(QueueItem sender, QueueEventArgs eventArgs)
        {
            _bytesTransferred = eventArgs.FileCopyEventArgs.BytesTransferred;
        }
        public QueueItem QueueItem
        {
            get
            {
                return _queueItem;
            }
        }
        public string Source
        {
            get
            {
                return _source;
            }
        }
        public string Target
        {
            get
            {
                return _target;
            }
        }
        public FileItem(QueueItem AQueueItem, string Source, string Target)
        {
            _queueItem = AQueueItem;
            _source = Source;
            _target = Target;
            _bytesTransferred = 0;
        }
    }

    public class QueueItem
    {
        private List<FileItem> _Copies;
        private AdvFileCopyQueue _q;
        private Thread _thread;
        private AutoResetEvent _Notification;
        private QueueEventArgs _eventArgs;
        public void Move(FileItem Source, bool up)
        {
            lock (_Copies)
            {
                int sourceIndex = _Copies.IndexOf(Source);
                int targetIndex = sourceIndex;
                if (up)
                {
                    targetIndex--;
                }
                else
                {
                    targetIndex++;
                }
                if (targetIndex >= 0 && targetIndex < _Copies.Count)
                {
                    var cpy = _Copies[sourceIndex];
                    _Copies.RemoveAt(sourceIndex);
                    _Copies.Insert(targetIndex, cpy);
                }
            }
        }
        public List<FileItem> Files
        {
            get
            {
                return _Copies;
            }
        }
        public FileItem AddQueue(string SourceFile, string TargetFile)
        {

            var fi = new FileItem(this, SourceFile, TargetFile);
            fi.Status = QueueStatus.Waiting;
            lock (_Copies)
            {                
                _Copies.Add(fi);
            }
            _Notification.Set();
            Run();
            return fi;
        }

        

        private void ThreadedRun(Thread AThread, params object[] args)
        {
            while (!_q.TheQueue.Terminated)
            {
                if (_Notification.WaitOne(1000))
                {
                    FileItem fi = null;
                    lock (_Copies)
                    {
                        if (_Copies.Count > 0)
                        {
                            fi = _Copies[0];
                            _Copies.RemoveAt(0);
                        }                        
                    }
                    if (fi != null)
                    {
                        var cpy = new AdvFileCopy();
                        _eventArgs.AdvFileCopy = cpy;
                        _eventArgs.TheFileItem = fi;
                        cpy.OnFileProgress += new FileCopyEvent(cpy_OnFileProgress);
                        cpy.CopyFile(fi.Source, fi.Target);
                    }
                }
            }
        }

        void cpy_OnFileProgress(object Sender, FileCopyEventArgs EventArgs)
        {
            _eventArgs.FileCopyEventArgs = EventArgs;
            _eventArgs.TheFileItem.DoFileProgress(this, _eventArgs);
            _q.DoFileProgress(this, _eventArgs);
           
        }
        public Thread CurrentThread
        {
            get
            {
                if (_thread == null)
                {
                    _thread = ThreadUtils.FastThread(ThreadedRun, this);
                }
                return _thread;
                
            }
        }
        public void Run()
        {
            if (!CurrentThread.IsAlive)
            {
                CurrentThread.Start();
            }
        }
        public void Pause()
        {
            if (CurrentThread.IsAlive)
            {
                //CurrentThread.s();
            }
            
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        private string _name;
        public QueueItem(AdvFileCopyQueue AQueue, string AName)
        {
            _q = AQueue;
            _Notification = new AutoResetEvent(false);
            _Copies = new List<FileItem>();
            _eventArgs = new QueueEventArgs();
            _name = AName;
        }

    }
    public class Queue : Dictionary<string, QueueItem>
    {
        public bool Terminated;
    }

    public class AdvFileCopyQueue
    {
        private Queue _queue;
        public Queue TheQueue
        {
            get
            {
                return _queue;
            }
        }
        private List<Thread> _threads;
        public AdvFileCopyQueue()
        {
            _queue = new Queue();
            _threads = new List<Thread>();
        }
        internal void DoFileProgress(QueueItem sender, QueueEventArgs eventArgs)
        {
            if (OnFileProgress != null)
            {
                OnFileProgress(sender, eventArgs);
            }
        }

        public event QueueEvent OnFileProgress;
        public FileItem QueueCopy(string AQueue, string SourceFile, string TargetFile)
        {
            QueueItem qi = null;
            if (!_queue.TryGetValue(AQueue, out qi))
            {
                qi = new QueueItem(this, AQueue);
                _queue[AQueue] = qi;
            }
            return qi.AddQueue(SourceFile, TargetFile);
        }
    }
}
