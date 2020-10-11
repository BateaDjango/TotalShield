using System;
using System.Drawing;
using System.Windows.Forms;

namespace TotalShield
{
    public partial class About : Form
    {
        Random rnd;
        public About()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            pictureBox1.SendToBack();
            pictureBox1.Visible = false;
            pictureBox1.Enabled = false;
            rnd = new Random();
            timer1.Enabled = true;
            timer1.Interval = 300;
            timer1.Start();
            timer1_Tick(null, null);


        }

        

        public void ToggleTimer(bool toggle_value)
        {
            timer1.Enabled = toggle_value;
            if (toggle_value)
                timer1_Tick(null, null);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int r = rnd.Next(0, 255);
            int g = rnd.Next(0, 255);
            int b = rnd.Next(0, 255);

            label1.BackColor = Color.FromArgb(r, g, b);
            label1.ForeColor = Color.FromArgb(255 - r, 255 - g, 255 - b);
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.BringToFront();
            pictureBox1.Visible = true;
            pictureBox1.Enabled = true;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.SendToBack();
            pictureBox1.Visible = false;
            pictureBox1.Enabled = false;
        }
    }
}
