using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TotalShieldSetup.TotalShieldUtils;

namespace TotalShieldSetup
{
    public partial class SetupForm : Form
    {

        string InstallLocation = null;
        Version InstalledVersion = null;
        Version EmbeddedProductVersion = null;

        bool finished = false;
        private Point mousePos;
        const int WS_MINIMIZEBOX = 0x20000;
        const int CS_DBLCLKS = 0x8;
        readonly Color borders_color = Color.FromArgb(24, 24, 24);
        const int borders_thickness = 5;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                cp.ClassStyle |= CS_DBLCLKS;
                return cp;
            }
        }
        public SetupForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;

            InitializeComponent();

        }

        private async void SetupForm_Load(object sender, EventArgs e)
        {
            button1.SendToBack();
            label4.Visible = false;
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            int textboxpadding = 8;
            richTextBox1.SetInnerMargins(textboxpadding, 0, textboxpadding, 0);
            richTextBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\" + AppPathName;
            progressBar1.Visible = false;


            string[] product = GetInstalledVersion();



            if (product == null)
            {
                button6.Enabled = false;


            }
            else
            {
                InstalledVersion = Version.Parse(product[0]);
                InstallLocation = product[1];
                EmbeddedProductVersion = Version.Parse(GetProductVersion());

                if (InstalledVersion == EmbeddedProductVersion)
                {
                    button5.Enabled = false;
                }
                else
                {
                    if (EmbeddedProductVersion > InstalledVersion)
                    {
                        button5.Text = "Update";
                        button1.Visible = false;

                    }

                }

            }



        }

        

        private void RunExeAfterInstall()
        {

            string[] info = GetInstalledVersion();
            if (info != null)
            {
                string exename = info[1] + @"\TotalShield.exe";

                DelayedStartApp(exename);
            }

        }


        private void btn_Close_Click(object sender, EventArgs e)
        {
            if (finished)
            {
                if (checkBox2.Checked && !button5.Text.Equals("Update"))
                {
                    RunExeAfterInstall();
                }
                Application.Exit();
            }

            else
                ExitSetup();
        }

        private void btn_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    richTextBox1.Text = fbd.SelectedPath + @"\" + AppPathName;

                }
            }
        }

        public void ToggleDuringInstall(bool on)
        {
            button1.Enabled = on;

            button2.Enabled = on;
            panel_MMC.Enabled = on;
            checkBox1.Enabled = on;
            checkBox2.Enabled = on;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (finished)
            {
                if (checkBox2.Checked && !button5.Text.Equals("Update"))
                {
                    RunExeAfterInstall();
                }

                Application.Exit();
            }
            else
            {
                InstallLocation = richTextBox1.Text;
                Init_install(InstallLocation);
            }
        }

        public async void Init_install(string path)
        {
            Program.unistall_path = null;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            button6.Enabled = false;
            finished = false;
            ToggleDuringInstall(false);

            Task.Run(() => StartInstall(path, checkBox1.Checked, this));

            for (int i = 0; i < 99; i++)
            {
                if (finished && i == 98)
                    progressBar1.Value += 2;
                else
                    progressBar1.Value += 1;
                await Task.Delay(25);
            }

            label4.BringToFront();
            label4.Visible = true;

            button1.Text = "Finish";
            button1.Visible = true;
            button1.Enabled = true;
            button1.BringToFront();
            panel_MMC.Enabled = true;
            if (InstallLocation == null)
                button6.Enabled = false;
            else
                button6.Enabled = true;
        }

        public void FinishInstall()
        {
            finished = true;

            if (progressBar1.Value == 99)
            {
                progressBar1.Value = 100;

                button1.Text = "Finish";
                button1.Enabled = true;
                button1.Visible = true;
                button1.BringToFront();
                panel_MMC.Enabled = true;
                label4.BringToFront();
                label4.Visible = true;
                if (InstallLocation == null)
                    button6.Enabled = false;
                else
                    button6.Enabled = true;
            }

        }

        

        

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point cur_pos = Control.MousePosition;
                cur_pos.Offset(mousePos.X, mousePos.Y);
                Location = cur_pos;
            }
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = new Point(-e.X, -e.Y);
        }

        private void SetupForm_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
           borders_color, borders_thickness, ButtonBorderStyle.Solid,
           borders_color, borders_thickness, ButtonBorderStyle.Solid,
           borders_color, borders_thickness, ButtonBorderStyle.Solid,
           borders_color, borders_thickness, ButtonBorderStyle.Solid);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button1.BringToFront();
            if (InstalledVersion != null && EmbeddedProductVersion > InstalledVersion)
            {
                progressBar1.BringToFront();
                label4.Text = "Update Successfully Completed!";
                label4.Visible = false;
                Init_install(InstallLocation);

                button5.Enabled = false;

            }
            else
                panel2.Visible = false;



        }

        private void button6_Click(object sender, EventArgs e)
        {
            InstallLocation = null;
            TotalMessage msg = new TotalMessage("Uninstall", "Are you sure you want to uninstall TotalShield?", MessageBoxButtons.YesNo);
            DialogResult res = msg.ShowDialog();
            msg.Dispose();
            if (res == DialogResult.Yes)
            {
                button1.Visible = false;
                button5.Enabled = false;
                button6.Enabled = false;
                progressBar1.BringToFront();

                label4.Visible = false;
                Init_install(InstallLocation);

                button5.Text = "Install";
                button5.Enabled = false;
                button6.Enabled = false;
                label4.Text = "Uninstall Successfully Completed!";



            }
        }


        public void ExitSetup()
        {
            TotalMessage msg = new TotalMessage("TotalShield Setup", "Are you sure you want to exit setup?", MessageBoxButtons.YesNo);
            DialogResult res = msg.ShowDialog();
            msg.Dispose();
            if (res == DialogResult.Yes)
            {

                Application.Exit();
            }

        }
    }
}
