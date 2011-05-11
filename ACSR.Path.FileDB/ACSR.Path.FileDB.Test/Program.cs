using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACSR.Path.FileDB.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new FileDB();
            db.Open("test.sqb");
        }
    }
}
