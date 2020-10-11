using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TotalShield
{
    public partial class ReportForm : Form
    {
        public int reportid;
        History parentform;
        Font font;
        readonly Color detected_color = Color.FromArgb(192, 0, 0);
        readonly Color date_color = Color.FromArgb(250, 243, 192);
        readonly Color clean_color = Color.Green;

        readonly Color borders_color = Color.FromArgb(100, 0, 0);
        readonly int borders_thickness = 3;
        private Point mousePos;

        public ReportForm(int rowid, History parent, string removed)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            parentform = parent;
            reportid = rowid;

            InitializeComponent();
            if (removed.Equals("Removed"))
                button1.Hide();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {

            this.ShowInTaskbar = true;
            ShowReport();
         
        }

        private void ShowReport()
        {
            AV_Reports reports = (AV_Reports)Settings.Get_Settings(typeof(AV_Reports));
            AV_Report report = reports.av_reports[reportid];

            label4.Text = report.file;
            label5.Text = report.time;

            Point offset = label3.Location;

            int distance = 60;
            int step = 15;
            int labelsize = 30;
            int x_step = 200;
        
            font = label3.Font;

            foreach (AV_Result result in report.av_results)
            {
                
                distance += labelsize;
                distance += step;
                Label avname = new Label();
                avname.Location = new Point(offset.X, offset.Y+distance);
                avname.Text = result.av_name + ": ";
                avname.Font = font;
                avname.ForeColor = date_color;
                avname.AutoSize = true;

                Label av_value = new Label();
                av_value.Location = new Point(offset.X + x_step, offset.Y + distance);
                av_value.Text = result.result;
                av_value.AutoSize = true;
                av_value.Font = font;
                if (result.result.Equals("Clean"))
                    av_value.ForeColor = clean_color;
                else
                    av_value.ForeColor = detected_color;

                this.Controls.Add(avname);
                this.Controls.Add(av_value);

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

        private void ReportForm_Paint(object sender, PaintEventArgs e)
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

        private void button1_Click(object sender, EventArgs e)
        {
            TotalMessage msg = new TotalMessage("Delete Threat", "Are you sure you want to delete this threat?", MessageBoxButtons.YesNo);
            DialogResult res = msg.ShowDialog();
           
            if (res == DialogResult.Yes)
            {
                string file = label4.Text;

                try
                {

                    if (File.Exists(file))
                        File.Delete(file);
                    else
                    {
                        TotalMessage not_removed_msg = new TotalMessage("Delete Threat Error", "File has been moved or already deleted!", MessageBoxButtons.OK);
                        not_removed_msg.ShowDialog();
                    }

                    parentform.SetFileRemovedInRow(reportid);
                }
                catch
                {
                    try
                    {
                        Process[] processCollection = Process.GetProcesses();
                        foreach (Process p in processCollection)
                        {
                            try
                            {
                                if (p.MainModule.FileName.Equals(file))
                                    p.Kill();
                            }
                            catch
                            { }

                        }
                        File.Delete(file);
                    }
                    catch
                    {
                        TotalMessage not_removed_msg = new TotalMessage("Delete Threat Error", "Something went wrong, could not delete some file(s).", MessageBoxButtons.OK);
                        not_removed_msg.ShowDialog();
                        return;
                    }
                }
                button1.Hide();

            }
        }

        private void btn_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
