using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;

using System.Diagnostics;

using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace PSV_Server
{


    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static public PSVServer serverForm = null;
        static public Server PSVServer = null;

        static public bool isRunning = true; 

        static void Main(string[] args)
        {
            //hides the console window.
            Console.Title = "PSVS_CONSOLE";
            IntPtr hWnd = FindWindow(null, "PSVS_CONSOLE"); //<< 2 lines hide console...
            ShowWindow(hWnd, 0);
            
            //using (Process p = Process.GetCurrentProcess())
            //p.PriorityClass = ProcessPriorityClass.High;


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //holds the ip  
            serverForm = new PSVServer();

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("PSV Pad Server");
            Console.WriteLine("Developed by AJ Moore");
            Console.WriteLine("Unorthodox Game Studios");
            Console.WriteLine("www.unorthodoxgamestudios.co.uk");
            Console.WriteLine("===============================");
            Console.WriteLine();


            Console.WriteLine("Enter the IP below on the PSVPAD vita app and press connect");  

            //Get the ip addresses associated with this host 
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    Console.WriteLine("Your IP: " + ip.ToString());
                    Console.WriteLine();
                    serverForm.ipAddress = ip.ToString();
                    break; 
                }

            }

            //creates the server 
            PSVServer = new Server();
            Application.Run(serverForm);

            Environment.Exit(0);

        }


        static public void CloseApplication()
        {
            serverForm.Close();
            
        }
    }
}
