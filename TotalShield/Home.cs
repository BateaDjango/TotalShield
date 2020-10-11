using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalShield
{
    public partial class Home : Form
    {
        private MainMenu parentform = null; 
        public Home(MainMenu parent)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            parentform = parent;
        }

        public void ToggleTimer(bool toggle_value)
        {
            timer1.Enabled = toggle_value;
            if (toggle_value)
                timer1_Tick(null, null);
        }

        

        private void Home_Load(object sender, EventArgs e)
        {
            updateLabels();

            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Start();
            timer1_Tick(null, null);
        }

        public void updateLabels()
        {
            if (Settings.isPremium())
                label5.Text = "Premium";
            else
            {
                if(Settings.isPublic())
                {
                    label5.Text = "Public";
                }
                else
                {
                    label5.Text = "No keys found";
                }
            }

            int avused = Settings.GetActiveAVs().Count;

            if (avused == 10)
                label7.Text = "All";
            else
                label7.Text = avused.ToString();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Scan scanform = parentform.getScanForm();
            if(scanform != null)
            {
                int progressvalue = scanform.getScanProgressValue();
                if(progressvalue == -1)
                {
                    label6.Text = "Not Scanning";
                }
                else
                {
                    label6.Text = "Scanning (" + progressvalue.ToString() + " %)";
                }
            }
            else
            {
                label6.Text = "Not Scanning";
            }
        }

        

        
    }
}
