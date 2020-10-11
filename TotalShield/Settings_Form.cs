using System;
using System.Windows.Forms;


namespace TotalShield
{
    public partial class Settings_Form : Form
    {

        public Settings_Form()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
        }

        

        public void loadPreferences()
        {
            Preferences pref = (Preferences)Settings.Get_Settings(typeof(Preferences));

            if (!pref.start_with_windows)
            {
                pictureBox21.Image = Properties.Resources.off;
                pictureBox22.Image = Properties.Resources.off;
            }
            else
            {
                pictureBox21.Image = Properties.Resources.on;
                if (pref.run_minimized)
                {
                    pictureBox22.Image = Properties.Resources.on;
                }
                else
                {
                    pictureBox22.Image = Properties.Resources.off;
                }
            }

            if (pref.scan_shortcut)
            {
                pictureBox25.Image = Properties.Resources.on;
            }
            else
            {
                pictureBox25.Image = Properties.Resources.off;
            }


            if (pref.run_shortcut)
            {
                pictureBox26.Image = Properties.Resources.on;
            }
            else
            {
                pictureBox26.Image = Properties.Resources.off;
            }


        }

        public void loadAcc()
        {
            if (Settings.isPremium())
            {
                pictureBox24.Image = Properties.Resources.on;
                pictureBox23.Image = Properties.Resources.off;

            }
            else
            {
                if (Settings.isPublic())
                {
                    pictureBox23.Image = Properties.Resources.on;
                    pictureBox24.Image = Properties.Resources.off;
                }
            }
        }

        public void loadAVs()
        {
            PictureBox tmp;
            foreach (Control control in this.Controls)
            {
                if (control.Tag == null || String.IsNullOrEmpty(control.Tag.ToString()))
                    continue;

                tmp = (PictureBox)control;
                if (Settings.isAVEnabled(control.Tag.ToString()))
                {

                    tmp.Image = Properties.Resources.on;
                }
                else
                {
                    tmp.Image = Properties.Resources.off;
                }

            }
        }

        private void ToggleAV(PictureBox pic)
        {
            AV_List list = Settings.GetAVList();

            int enabled_count = 0;
            string avname = null;
            foreach (AV antivirus in list.av)
            {
                if (antivirus.enabled)
                {
                    enabled_count++;
                    avname = antivirus.name;
                }

            }

            if (enabled_count == 1 && avname.Equals(pic.Tag.ToString()))
                return;


            string tmptag = pic.Tag.ToString(); bool enabled = !Settings.isAVEnabled(tmptag);
            Settings.Set_AV(tmptag, enabled);

            pic.Image = enabled ? Properties.Resources.on : Properties.Resources.off;

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            ToggleAV((PictureBox)sender);
        }

        private void pictureBox21_Click(object sender, EventArgs e)
        {
            Preferences pref = (Preferences)Settings.Get_Settings(typeof(Preferences));

            if (!pref.start_with_windows)
            {
                if (!Settings.IsStartup())
                    Settings.AddtoStartup();
                pref.start_with_windows = true;
                pictureBox21.Image = Properties.Resources.on;

            }
            else
            {
                if (Settings.IsStartup())
                    Settings.RemoveFromStartup();
                pref.start_with_windows = false;
                pref.run_minimized = false;
                pictureBox21.Image = Properties.Resources.off;
                pictureBox22.Image = Properties.Resources.off;
            }
            Settings.Save_Settings(pref);
        }

        private void pictureBox22_Click(object sender, EventArgs e)
        {
            Preferences pref = (Preferences)Settings.Get_Settings(typeof(Preferences));

            if (!pref.run_minimized)
            {
                if (!pref.start_with_windows)
                    return;

                pictureBox22.Image = Properties.Resources.on;
                pref.run_minimized = true;

            }
            else
            {
                pictureBox22.Image = Properties.Resources.off;
                pref.run_minimized = false;
            }

            Settings.Save_Settings(pref);
        }

        private void pictureBox23_Click(object sender, EventArgs e)
        {
            var result = Settings.ToggleActiveKey(false);
            if (result == 0)
                return;

            if (result == 1)
            {
                pictureBox23.Image = Properties.Resources.on;
                pictureBox24.Image = Properties.Resources.off;
            }
            else
            {
                pictureBox23.Image = Properties.Resources.off;
                pictureBox24.Image = Properties.Resources.on;
            }


        }

        private void pictureBox24_Click(object sender, EventArgs e)
        {
            var result = Settings.ToggleActiveKey(true);
            if (result == 0)
                return;

            if (result == 1)
            {
                pictureBox24.Image = Properties.Resources.on;
                pictureBox23.Image = Properties.Resources.off;
            }
            else
            {
                pictureBox24.Image = Properties.Resources.off;
                pictureBox23.Image = Properties.Resources.on;
            }

        }

        

        private void pictureBox25_Click(object sender, EventArgs e)
        {
            Preferences pref = (Preferences)Settings.Get_Settings(typeof(Preferences));

            if (pref.scan_shortcut)
            {

                Settings.RemoveFileContext(true);
                pictureBox25.Image = Properties.Resources.off;
                pref.scan_shortcut = false;
                Settings.Save_Settings(pref);


            }
            else
            {
                if (Settings.AddFileContext(true))
                {
                    pictureBox25.Image = Properties.Resources.on;
                    pref.scan_shortcut = true;
                    Settings.Save_Settings(pref);
                }

            }

        }

        private void pictureBox26_Click(object sender, EventArgs e)
        {
            Preferences pref = (Preferences)Settings.Get_Settings(typeof(Preferences));

            if (pref.run_shortcut)
            {

                Settings.RemoveFileContext(false);
                pictureBox26.Image = Properties.Resources.off;
                pref.run_shortcut = false;
                Settings.Save_Settings(pref);


            }
            else
            {
                if (Settings.AddFileContext(false))
                {
                    pictureBox26.Image = Properties.Resources.on;
                    pref.run_shortcut = true;
                    Settings.Save_Settings(pref);
                }

            }

        }
    }
}
