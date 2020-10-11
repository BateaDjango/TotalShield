using System;
using System.Drawing;
using System.Windows.Forms;

namespace TotalShield
{
    public partial class TotalMessage : Form
    {
        readonly Color borders_color = Color.FromArgb(100, 0, 0);
        readonly int borders_thickness = 3;
        private Point mousePos;

        public TotalMessage(string title, string message, MessageBoxButtons buttons)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            label1.Text = title;
            label2.Text = message;

            if (buttons == MessageBoxButtons.YesNo)
            {
                button3.Hide();
            }
            else
            {
                button1.Hide();
                button2.Hide();
            }
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawIcon(Properties.Resources.AppIcon, 0, 0);
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void TotalMessage_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
           borders_color, borders_thickness, ButtonBorderStyle.Solid,
           borders_color, borders_thickness, ButtonBorderStyle.Solid,
           borders_color, borders_thickness, ButtonBorderStyle.Solid,
           borders_color, borders_thickness, ButtonBorderStyle.Solid);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point cur_pos = Control.MousePosition;
                cur_pos.Offset(mousePos.X, mousePos.Y);
                Location = cur_pos;
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = new Point(-e.X, -e.Y);
        }

        private void btn_Close_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
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

        
    }
}
