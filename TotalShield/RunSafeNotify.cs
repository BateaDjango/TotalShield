using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalShield
{
    public partial class RunSafeNotify : Form
    {
        MainMenu mainform;
        public RunSafeNotify(MainMenu main)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            mainform = main;
            InitializeComponent();
            StartPosition = FormStartPosition.Manual;
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width, Screen.PrimaryScreen.WorkingArea.Height - Height);
        }

        private void RunSafeNotify_Load(object sender, EventArgs e)
        {
            ToggleButton(false);
        }

        public void SetLabelMsg(string msg, Color clr)
        {
            label2.Text = msg;
            label2.ForeColor = clr;
        }

        public void ToggleButton(bool value)
        {
            if (value)
                button1.Show();
            else
                button1.Hide();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawIcon(Properties.Resources.AppIcon, 0, 0);
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        public async void DelayedClose(int ms)
        {
            await Task.Delay(ms);
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (mainform.ShowInTaskbar == false)
                mainform.ShowForm();
            else
                mainform.WindowState = FormWindowState.Normal;

            this.Close();
        }
    }
}
