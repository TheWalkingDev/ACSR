using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ACSR.Core.Processes
{
    public enum EnumAppMode 
    {
        Unknown = 0,
        Console = 1,
        GUI = 2
    }
    public class MixedModeApplication
    {
        public delegate void RunEvent(object sender);
         
        public event  RunEvent OnRunGUI;
        public event  RunEvent OnRunConsole;

    

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        public EnumAppMode ApplicationMode { get; set;}


        [STAThread]
        public void Run()
        {        
            if (ApplicationMode == EnumAppMode.GUI)
            {
                /*MessageBox.Show("Welcome to GUI mode");
                Application.EnableVisualStyles();

                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new Form1());*/
                OnRunGUI(this);
            }
            else if (ApplicationMode == EnumAppMode.Console)
            {

                //Get a pointer to the forground window.  The idea here is that
                //IF the user is starting our application from an existing console
                //shell, that shell will be the uppermost window.  We'll get it
                //and attach to it
                IntPtr ptr = GetForegroundWindow();

                int  u;

                GetWindowThreadProcessId(ptr, out u);

                Process process = Process.GetProcessById(u);

                if (process.ProcessName == "cmd" )    //Is the uppermost window a cmd process?
                {
                    AttachConsole(process.Id);
                   
                    //we have a console to attach to ..
                    Console.WriteLine("hello. It looks like you started me from an existing console.");
                }
                else
                {
                    //no console AND we're in console mode ... create a new console.

                    AllocConsole();
                    OnRunConsole(this);
                    /*

                    Console.WriteLine(@"hello. It looks like you double clicked me to start
                   AND you want console mode.  Here's a new console.");
                    Console.WriteLine("press any key to continue ...");
                    Console.ReadLine();       */
                }

                FreeConsole();
            }
        }

    }
}
