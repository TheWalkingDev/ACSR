using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ACSR.Core.Streams
{
    public static class StreamExtensions
    {
        public static byte[] ReadSizedObject(this Stream s) // Note parameter declaration.
        {
            var buf = new byte[sizeof(Int32)];
            s.Read(buf, 0, buf.Length);
            var size = BitConverter.ToInt32(buf, 0);
            buf = new byte[size];
            s.Read(buf, 0, buf.Length);
            return buf;
        }
        public static string ReadSizedObjectAsString(this Stream s) // Note parameter declaration.
        {
            return ASCIIEncoding.ASCII.GetString(ReadSizedObject(s));
        }
    }

   
}
