using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACSR.Core.Disk
{
    public class FileCopyEventArgs
    {
        public long BytesTransferred;
        public long BytesLeft;
        public byte[] Buffer;
        public Stream Source;
        public Stream Destination;
        public FileCopyEventAction Action;
    }

    public enum FileCopyEventAction
    {
        Unknown = 0,
        Abort = 1,
        Continue = 2
    }

    public enum FileCopyResult
    {
        Unknown = 0,
        Success = 1,
        Aborted = 2
    }

    public enum FileExistsAction
    {
        Unknown = 0,
        Overwrite = 1,
        OverwriteOlder = 2,
        Resume = 3
    }

    public class AdvCopyFileOptions
    {
        public FileExistsAction DefaultFileExistsAction;
        public long BufferSize = 1000000;
        public AdvCopyFileOptions()
        {
            DefaultFileExistsAction = FileExistsAction.Resume;
        }
    }

    public delegate void FileCopyEvent(object Sender, FileCopyEventArgs EventArgs);

    public class AdvFileCopy
    {

        public AdvCopyFileOptions Options
        {
            get
            {
                return _options;
            }
        }

        private string _targetFile;
        private string _sourceFile;
        public string SourceFile
        {
            get
            {
                return _sourceFile;
            }
        }
        public string TargetFile
        {
            get
            {
                return _targetFile;
            }
        }
        private AdvCopyFileOptions _options;
        private FileCopyEventArgs _EventArgs;
        public event FileCopyEvent OnFileProgress;
        public AdvFileCopy()
        {
            _options = new AdvCopyFileOptions();
            _EventArgs = new FileCopyEventArgs();

        }

        public FileCopyResult CopyFile(string Source, string Target)
        {
            _sourceFile = Source;
            _targetFile = Target;
            bool resume = false;
            _EventArgs.Action = FileCopyEventAction.Continue;
            FileCopyResult result = FileCopyResult.Success;
            using (FileStream source = new FileStream(Source, FileMode.Open, FileAccess.Read))
            {
                long total = source.Length;
                long left = total;
                if (File.Exists(Target))
                {
                    switch (Options.DefaultFileExistsAction)
                    {
                        case FileExistsAction.Overwrite:
                            {
                                File.Delete(Target);
                                break;
                            }
                        case FileExistsAction.OverwriteOlder:
                            {
                                if (File.GetLastWriteTime(Target) < File.GetLastWriteTime(Source))
                                {
                                    File.Delete(Target);
                                }
                                break;
                            }
                        case FileExistsAction.Resume:
                            {
                                resume = true;
                                break;
                            }
                    }
                }

                using (FileStream target = new FileStream(Target, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    long bytesTransferred = 0;
                    if (resume)
                    {
                        target.Seek(0, SeekOrigin.End);
                        left -= target.Length;
                        bytesTransferred += target.Length;
                    }

                    while (left > 0)
                    {
                        byte[] buffer;
                        if (left > Options.BufferSize)
                        {
                            buffer = new byte[Options.BufferSize];
                        }
                        else
                        {
                            buffer = new byte[left];
                        }
                        left -= buffer.Length;
                        bytesTransferred += buffer.Length;

                        source.Read(buffer, 0, buffer.Length);
                        target.Write(buffer, 0, buffer.Length);
                        _EventArgs.BytesLeft = left;
                        _EventArgs.BytesTransferred = bytesTransferred;

                        if (OnFileProgress != null)
                            OnFileProgress(this, _EventArgs);
                        if (_EventArgs.Action == FileCopyEventAction.Abort)
                        {
                            result = FileCopyResult.Aborted;
                            break;
                        }
                    }

                }
                if (result != FileCopyResult.Aborted)
                {
                    File.SetCreationTime(Target, File.GetCreationTime(Source));
                    File.SetAttributes(Target, File.GetAttributes(Source));
                }
            }
            return result;
        }
    }
}