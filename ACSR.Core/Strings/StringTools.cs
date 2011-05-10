using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACSR.Core.Strings
{
    public static class StringTools
    {


        public static string FileToString(string FileName)
        {
            using  (FileStream fs = new FileStream(FileName, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        public static void StringToFile(string FileName, string S)
        {
            using (FileStream fs = new FileStream(FileName, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(S);
                }
            }
        }
        public static MemoryStream StringToStream(string value)
        {
            var ms = new MemoryStream();
            var buf = ASCIIEncoding.ASCII.GetBytes(value);
            ms.Write(buf, 0, buf.Length );
            ms.Position = 0;
            return ms;
        }
        public static string StreamToString(Stream s)
        {
            byte[] buf = new byte[s.Length - s.Position];
            s.Read(buf, 0, buf.Length);
            return ASCIIEncoding.ASCII.GetString(buf, 0, buf.Length);
        }
    }
}
