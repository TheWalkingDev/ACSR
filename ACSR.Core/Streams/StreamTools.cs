using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ACSR.Core.Streams
{
    public static class StreamTools
    {
        public static void StreamToFile(string FileName, Stream s)
        {
            using (FileStream fs = new FileStream(FileName, FileMode.Create))
            {
                byte[] buf = new byte[s.Length - s.Position];
                s.Read(buf, 0, buf.Length);
                fs.Write(buf, 0, buf.Length);

            }
        }
        public static void FileToStream(string FileName, Stream s)
        {
            using (FileStream fs = new FileStream(FileName, FileMode.Open))
            {
                byte[] buf = new byte[fs.Length];
                fs.Read(buf, 0, buf.Length);
               
                s.Write(buf, 0, buf.Length);

            }

        }
        public static MemoryStream FileToMemoryStream(string FileName)
        {
            var ms = new MemoryStream();
            FileToStream(FileName, ms);
            ms.Position = 0;
            return ms;
        }

    }
}
