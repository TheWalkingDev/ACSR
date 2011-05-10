using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ACSR.PythonScripting
{
    public class BufferedStream : Stream
    {


        public event FlushBufferEvent OnFlushBuffer;
        //private static readonly Peresys.AtMarket.Interfaces.Logging.ILog Logger = Peresys.AtMarket.Logging.LogManager.GetLogger(typeof(GuiStream));

        private Stopwatch _sw;
        private MemoryStream _ms;
        private int _bufferSize;
        private int _bufferTime;
        public BufferedStream(int BufferSize, int MaxBufferTime)
            : base()
        {
            _bufferSize = BufferSize;
            _bufferTime = MaxBufferTime;
            _sw = new Stopwatch();
            _sw.Start();
            _ms = new MemoryStream();
        }
        #region Ignore Read and Seek
        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }
        public override void Flush() { } // do nothing
        public override long Length { get { throw new NotSupportedException(); } }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        #endregion

        public override bool CanWrite { get { return true; } }
        public void FlushBuffer()
        {
            _ms.Position = 0;
            var buf = new byte[_ms.Length];
            _ms.Read(buf, 0, buf.Length);
            _sw.Reset();
            if (OnFlushBuffer != null)
            {
                OnFlushBuffer(buf);
            }
            _ms = new MemoryStream();
            _sw.Start();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            _ms.Write(buffer, offset, count);
            if (_sw.ElapsedMilliseconds >= _bufferTime || _ms.Length >= _bufferSize)
            {
                FlushBuffer();
            }

        }
    }
}
