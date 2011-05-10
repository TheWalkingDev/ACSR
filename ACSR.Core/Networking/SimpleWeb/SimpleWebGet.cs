using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ACSR.Core.Networking.SimpleWeb
{
    public class SimpleWebGet
    {
        public static string Get(string URL)
        {
            return Get(URL, null, 0);
        }
        public static string Get(string URL, string Proxy, int Port)
        {
            return Get(URL, Proxy, Port, 0);
        }


        public static string Get(string URL, string Proxy, int Port, int MaxData) 
        {
            return Get(URL, Proxy, Port, MaxData, null, null);
        }
        public static string Get(string URL, string Proxy, int Port, int MaxData, string User, string Pass)
        {
            StringBuilder sb = new StringBuilder();

            // used on each read operation
            byte[] buf = new byte[8192];

            // prepare the web page we will be asking for
            HttpWebRequest request = (HttpWebRequest)
                                     WebRequest.Create(URL);

            if (User != null)
            {
                //Console.WriteLine("Authenticating...");
                request.Credentials = new NetworkCredential(User, Pass);
                request.PreAuthenticate = true;
            }

            if (!string.IsNullOrEmpty(Proxy))
            {
                request.Proxy = new WebProxy(Proxy, Port);
            }

            // execute the request
            HttpWebResponse response = (HttpWebResponse)
                                       request.GetResponse();

            // we will read data via the response stream

            Stream resStream = response.GetResponseStream();

            string tempString = null;
            int count = 0;
            int dataRead = 0;
            do
            {
                // fill the buffer with data
                int bufSize;
                if (MaxData > 0 && buf.Length > MaxData)
                    bufSize = MaxData;
                else
                {
                    bufSize = buf.Length;
                }
                count = resStream.Read(buf, 0, bufSize);
                dataRead += count;
                // make sure we read some data
                if (count != 0)
                {
                    // translate from bytes to ASCII text
                    tempString = Encoding.ASCII.GetString(buf, 0, count);

                    // continue building the string
                    sb.Append(tempString);
                }
                if (MaxData > 0 && dataRead > MaxData)
                {
                    resStream.Close();
                    break;
                }
            }
            while (count > 0); // any more data to read?

            // print out page source
            return sb.ToString();

        }
        public byte[] GetData(string URL, string Proxy=null, int Port=0)
        {

            // prepare the web page we will be asking for
            HttpWebRequest request = (HttpWebRequest)
                                     WebRequest.Create(URL);
            if (!string.IsNullOrEmpty(Proxy))
            {
                request.Proxy = new WebProxy(Proxy, Port);
            }

            // execute the request
            HttpWebResponse response = (HttpWebResponse)
                                       request.GetResponse();

            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();
            var buf = new byte[64000];
            var result = new MemoryStream();
            int count = 0;
            int dataRead = 0;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                result.Write(buf, 0, count);
                dataRead += count;
            }
            while (count > 0); // any more data to read?

            // print out page source
            buf = new byte[result.Length];
            result.Seek(0, SeekOrigin.Begin);
            
            result.Read(buf, 0, buf.Length);
            return buf;
        }
    }
}