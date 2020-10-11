using System;
using System.Threading;
using System.Windows.Forms;

namespace TotalShield
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{9F846E12-DBA8-47AE-BAAA-64321813F9D3}");

        [STAThread]
        static void Main(string[] args)
        {
            int args_count = Environment.GetCommandLineArgs().Length;

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    Settings.EncryptKeys();

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    if (args.Length == 0)
                    {

                        if (args_count > 2)
                        {
                            string folderName = Environment.GetCommandLineArgs()[1];
                            string scan = Environment.GetCommandLineArgs()[2];




                            Application.Run(new MainMenu(folderName, Int32.Parse(scan)));
                        }
                        else
                            Application.Run(new MainMenu("NotStartup"));
                    }
                    else
                    {
                        if (args_count > 2)
                        {
                            string folderName = Environment.GetCommandLineArgs()[1];
                            string scan = Environment.GetCommandLineArgs()[2];

                            Application.Run(new MainMenu(folderName, Int32.Parse(scan)));
                        }
                        else
                            Application.Run(new MainMenu(args[0]));
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                IntPtr hWnd = (IntPtr)Com_Interface.FindWindow(null, "TotalShield");
                if (hWnd == IntPtr.Zero)
                {
                    Thread.Sleep(300);
                    hWnd = (IntPtr)Com_Interface.FindWindow(null, "TotalShield");
                }


                if (args_count > 2)
                {

                    string folderName = Environment.GetCommandLineArgs()[1];
                    string scan = Environment.GetCommandLineArgs()[2];

                    string args2 = scan + folderName;



                    Com_Interface.sendWindowsStringMessage((int)hWnd, 0, args2);




                }
                else
                {

                    Com_Interface.SendMessage(
                    hWnd,
                    Com_Interface.WM_SHOWME,
                    IntPtr.Zero,
                    IntPtr.Zero);
                }
            }



        }
    }
}
