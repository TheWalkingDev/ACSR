using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ACSR.Core.Strings
{
    public static class StringWriterExtensions
    {
        public static void WriteName(this StringWriter SS, string Name) // Note parameter declaration.
        {            
            var bSize = BitConverter.GetBytes((Int32)Name.Length);
            SS.Write(ASCIIEncoding.ASCII.GetString(bSize, 0, bSize.Length));            
            SS.Write(Name);
        }
        public static void WriteBool(this StringWriter SS, bool Value)
        {
            var bSize = BitConverter.GetBytes(Value);
            SS.Write(ASCIIEncoding.ASCII.GetString(bSize, 0, bSize.Length));
        }
        public static void WriteInt(this StringWriter SS, Int32 Value)
        {
            var bSize = BitConverter.GetBytes(Value);
            SS.Write(ASCIIEncoding.ASCII.GetString(bSize, 0, bSize.Length));
        }
    }
}
