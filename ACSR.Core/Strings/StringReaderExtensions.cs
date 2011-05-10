using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ACSR.Core.Strings
{
    public static class StringReaderExtensions
    {
        public static string ReadName(this StringReader SS) // Note parameter declaration.
        {
            var buf = new char[sizeof(Int32)];
            SS.Read(buf, 0, buf.Length);
            var size = BitConverter.ToInt32(ASCIIEncoding.ASCII.GetBytes(buf), 0);
            buf = new char[size];
            SS.Read(buf, 0, buf.Length);

            return ASCIIEncoding.ASCII.GetString(ASCIIEncoding.ASCII.GetBytes(buf));
        }
        public static bool ReadBool(this StringReader SS)
        {
            var buf = new char[sizeof(bool)];
            SS.Read(buf, 0, buf.Length);
            return BitConverter.ToBoolean(ASCIIEncoding.ASCII.GetBytes(buf), 0);
        }

        public static int ReadInt(this StringReader SS)
        {
            var buf = new char[sizeof(Int32)];
            SS.Read(buf, 0, buf.Length);
            return BitConverter.ToInt32(ASCIIEncoding.ASCII.GetBytes(buf), 0);
        }

    }
}
