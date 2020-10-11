using System;
using System.Windows.Forms;

namespace TotalShieldSetup
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
        static void OnProcessExit(object sender, EventArgs e)
        {
            if (unistall_path != null)
            {
                TotalShieldUtils.DelayedDeleteFolder(unistall_path);
            }
        }
        public static string unistall_path = null;
    }
}
