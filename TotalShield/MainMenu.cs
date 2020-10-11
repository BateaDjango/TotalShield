using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirusTotalNet;
using VirusTotalNet.Objects;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;

namespace TotalShield
{
    public partial class MainMenu : Form
    {
        private string startup = null;
        private string context_filescan;
        private int context_scanid;
        public bool opened_runsafe = false;

        public MainMenu(string is_startup)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            startup = is_startup;

        }


        public MainMenu(string folder_name, int scan)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();

            context_filescan = folder_name;
            context_scanid = scan;
            opened_runsafe = true;

        }

        public int last_history = 1;

        private Form openedForm = null;

        private Home home_form = null;
        private Scan scan_form = null;
        private History history_Form = null;
        private Account account_form = null;
        private Settings_Form settings_form = null;
        private About about_form = null;


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Com_Interface.WM_SHOWME)
            {
                ShowInstance();
            }
            else
            {

                if(m.Msg == Com_Interface.WM_COPYDATA)
                {
                     Com_Interface.COPYDATASTRUCT st = (Com_Interface.COPYDATASTRUCT) Marshal.PtrToStructure(m.LParam,typeof(Com_Interface.COPYDATASTRUCT));

                     string tmp = st.lpData;
                
                    HandleFileContext(tmp.Substring(1), Int32.Parse(tmp[0].ToString()));
                  
                }
                   
               
            }
            base.WndProc(ref m);
        }

        private void HandleFileContext(string filepath, int command)
        {
         
            btn_Scan_Click(null, null);

            scan_form.AddToScan(filepath, command);

        }

        private async void ShowInstance()
        {
              ShowForm();
              if(this.ShowInTaskbar == false)
               {
                   ShowForm();
               }
              else
              {
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                }



                  TopMost = true;
                  await Task.Delay(100);

                  TopMost = false;
              }
           

               
            
        

        }

        public Scan getScanForm()
        {
            return scan_form;
        }

        public void openForm(Form form)
        {
            if (openedForm != null)
            {
                openedForm.Hide();
                panel_MainForm.Controls.Remove(openedForm);
            }

            openedForm = form;
            form.TopLevel = false;
            panel_MainForm.Controls.Add(form);
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panel_MainForm.Tag = form;
            form.BringToFront();
           
            form.Show();
        }


        const int WS_MINIMIZEBOX = 0x20000;
        const int CS_DBLCLKS = 0x8;
        private Point mousePos;
        readonly Color borders_color = Color.FromArgb(24, 24 ,24);
        const int borders_thickness = 5;


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                cp.ClassStyle |= CS_DBLCLKS;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public void focusSettings()
        {
            btn_Settings.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

          
            this.notifyIcon.Visible = true;
            notifyIcon.Icon = Properties.Resources.AppIcon;
            notifyIcon.ContextMenuStrip = trayMenuStrip;
           
            if(startup  == null)
            {
                HandleFileContext(context_filescan, context_scanid); 
            }
            else
            {
                btn_Home_Click(null, null);
                if (startup.Equals("startup"))
                {
                    Preferences pref = (Preferences)Settings.Get_Settings(typeof(Preferences));
                    if (pref.run_minimized)
                        HideForm();
                }
            }
          

          

        }

        private void btn_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            

        }

        public void FocusHistory()
        {
            btn_History.Focus();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            HideForm();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowForm();
        }

        public void ShowForm()
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            
        }

        public void HideForm()
        {
            this.ShowInTaskbar = false;
            this.Visible = false;
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            TotalMessage msg = new TotalMessage(Settings.AppName, "Are you sure you want to exit?", MessageBoxButtons.YesNo);
            if(msg.ShowDialog() == DialogResult.Yes)
            {
                notifyIcon.Visible = false;
                notifyIcon.Icon = null;
                notifyIcon.Dispose();
                msg.Dispose();
                Application.Exit();
            }
            msg.Dispose();
        }

        private void homeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        
            ShowForm();
            btn_Home_Click(null, null);

        }

        private void btn_Home_Click(object sender, EventArgs e)
        {
            if(home_form == null)
            {
                home_form = new Home(this);
               
            }
            if (openedForm != home_form)
            {
                if (about_form != null)
                    about_form.ToggleTimer(false);

                openForm(home_form);
                home_form.ToggleTimer(true);

                home_form.updateLabels();
            }

        }

        private void btn_Scan_Click(object sender, EventArgs e)
        {
            if (scan_form == null)
            {
                scan_form = new Scan(this);
            }
            if (openedForm != scan_form)
            {
                if (home_form != null)
                    home_form.ToggleTimer(false);
                if (about_form != null)
                    about_form.ToggleTimer(false);
                openForm(scan_form);
                scan_form.ResetScan();
            }
        }

        public void btn_History_Click(object sender, EventArgs e)
        {
            if (history_Form == null)
            {
                history_Form = new History(this);
                
            }

            bool isFilenum = sender != null && sender.GetType() == typeof(FilesNum);

            if (openedForm != history_Form || (!isFilenum && last_history == 1))
            {
                if (about_form != null)
                    about_form.ToggleTimer(false);

                if (home_form != null)
                    home_form.ToggleTimer(false);
                openForm(history_Form);

                if (isFilenum)
                {
                    FilesNum number = (FilesNum)sender;
                    history_Form.Reload(number);
               
                }
                else
                {
                    history_Form.Reload(new FilesNum(0));
                }
            }
          
        }

        private void btn_Account_Click(object sender, EventArgs e)
        {
            if (account_form == null)
            {
                account_form = new Account();
            }
            if (openedForm != account_form)
            {
                if (about_form != null)
                    about_form.ToggleTimer(false);

                if (home_form != null)
                    home_form.ToggleTimer(false);
                openForm(account_form);
                account_form.RefreshPictureboxes();
                account_form.LoadKeys();
            }
        }

        private void btn_Settings_Click(object sender, EventArgs e)
        {
            if (settings_form == null)
            {
                settings_form = new Settings_Form();
            }
            if (openedForm != settings_form)
            {
                if (about_form != null)
                    about_form.ToggleTimer(false);

                if (home_form != null)
                    home_form.ToggleTimer(false);

                openForm(settings_form);
                settings_form.loadAcc();
                settings_form.loadAVs();
                settings_form.loadPreferences();
            }
        }

        private void btn_About_Click(object sender, EventArgs e)
        {
            if (about_form == null)
            {
                about_form = new About();
            }
            if (openedForm != about_form)
            {
                if (home_form != null)
                    home_form.ToggleTimer(false);
                openForm(about_form);
                about_form.ToggleTimer(true);
            }
        }

      
        private void scanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
            btn_Scan_Click(null, null);
        }

        private void historyStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
            btn_History_Click(null, null);
        }

        private void accountStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
            btn_Account_Click(null, null);
        }

        private void settingsStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
            btn_Settings_Click(null, null);
        }

        private void aboutStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
            btn_About_Click(null, null);
        }

        private void exitStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Icon = null;
            notifyIcon.Dispose();
            Application.Exit();
        }

        

        

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point cur_pos = Control.MousePosition;
                cur_pos.Offset(mousePos.X-menuPanel.Width, mousePos.Y);
                Location = cur_pos;
            }
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = new Point(-e.X, -e.Y);
        }

        

        

      
        private void MainMenu_Paint(object sender, PaintEventArgs e)
        {
           
            ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, 
            borders_color, borders_thickness, ButtonBorderStyle.Solid,
            borders_color, borders_thickness, ButtonBorderStyle.Solid,
            borders_color, borders_thickness, ButtonBorderStyle.Solid,
            borders_color, borders_thickness, ButtonBorderStyle.Solid);
          
        }

       

        

        private void Panel2_Paint(object sender, PaintEventArgs e)
        {
          
            ControlPaint.DrawBorder(e.Graphics, this.panel_MainForm.ClientRectangle,
            borders_color, borders_thickness, ButtonBorderStyle.Solid,
            borders_color, 0, ButtonBorderStyle.Solid,
            borders_color, 0, ButtonBorderStyle.Solid,
            borders_color, 0, ButtonBorderStyle.Solid);
        }



        

        
    }

    public class FilesNum
    {
        public FilesNum(int number)
        {
            num = number;
        }

        public int num;
        public int threats;
        public string duration;
    }
}
