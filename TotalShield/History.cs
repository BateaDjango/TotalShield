using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalShield
{
    public partial class History : Form
    {
        
        MainMenu parentform = null;
        readonly Color detected_color = Color.FromArgb(192, 0, 0);
        readonly Color date_color = Color.FromArgb(250, 243, 192);
        readonly Color clean_color = Color.Green;
        readonly Color label_color = Color.FromArgb(126, 126, 126);



        public History(MainMenu parent)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            parentform = parent;
            InitializeComponent();
          
        }

        public void TogglePanel(Panel panel, bool show)
        {
            if (show)
                panel.BringToFront();
            else
                panel.SendToBack();
            panel.Visible = show;
        }

        public void Reload(FilesNum files_info)
        {
            
            int files_num = files_info.num;

            if (parentform.last_history == 0 && files_num == 0)
            return;
        
            if (files_num != 0)
            {
                label1.Text = "Scan Results";
                TogglePanel(panel2, true);
                TogglePanel(panel3, false);
              
                TogglePanel(panel5, false);

                label3.Text = files_num.ToString();
                int threatsfound = files_info.threats;
                
                if (threatsfound > 0)
                    TogglePanel(panel4, true);
                else
                    TogglePanel(panel4, false);

                label4.Text = threatsfound.ToString();
                if (threatsfound == 0)
                    label4.ForeColor = clean_color;
                else
                    label4.ForeColor = detected_color;
                label6.Text = files_info.duration;

            }
            else
            {
                label1.Text = "Scan History";
                TogglePanel(panel2, false);
                TogglePanel(panel3, true);
                TogglePanel(panel4, false);
                TogglePanel(panel5, true);
            }


            label2.Visible = false;
            try
            {
               
                 AV_Reports reports = (AV_Reports)Settings.Get_Settings(typeof(AV_Reports));
                if (reports.av_reports.Count == 0)
                {
                    label2.Visible = true;
                    TogglePanel(panel5, false);
                }
               
                LoadGridValues(reports, files_num);
                   
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                label2.Visible = true;
            }

            

            if (files_num == 0)
                parentform.last_history = 0;
            else
                parentform.last_history = 1;

        }

        private void LoadGridValues(AV_Reports reports, int files_num)
        {

            dataGridView1.Rows.Clear();
            
            SHA256 sha256obj = SHA256.Create();

            if (files_num == 0)
                files_num = reports.av_reports.Count;

            bool threat;
            string filehash;
            int i = 0;
            foreach( AV_Report report in reports.av_reports)
            {
                if (i == files_num)
                    break;
                threat = VT_API.IsThreat(report);
                

                if (!File.Exists(report.file))
                {
                    if (!threat)
                    {
                        dataGridView1.Rows.Add(report.time, report.file, "Removed");
                        dataGridView1.Rows[i].Cells[2].Style.ForeColor = label_color;
                    }
                    else
                    {
                        dataGridView1.Rows.Add(report.time, report.file, "Clean");
                        dataGridView1.Rows[i].Cells[2].Style.ForeColor = clean_color;
                        dataGridView1.Rows[i].Cells[3].Value = Properties.Resources.theme_background;
                    }
                }
                else
                {

                        filehash = VT_API.BytesToHexString(sha256obj.ComputeHash(File.ReadAllBytes(report.file)));

                        if (threat && filehash.Equals(report.hash))
                        {
                            dataGridView1.Rows.Add(report.time, report.file, "Detected");
                            dataGridView1.Rows[i].Cells[2].Style.ForeColor = detected_color;
                        }
                        else
                        {
                            if (threat)
                            {
                                dataGridView1.Rows.Add(report.time, report.file, "Removed");
                                dataGridView1.Rows[i].Cells[2].Style.ForeColor = label_color;
                            }
                            else
                            {
                                dataGridView1.Rows.Add(report.time, report.file, "Clean");
                                dataGridView1.Rows[i].Cells[2].Style.ForeColor = clean_color;
                                dataGridView1.Rows[i].Cells[3].Value = Properties.Resources.theme_background;
                            }
                        }
                    
                  
                }

                dataGridView1.Rows[i].Cells[0].Style.ForeColor = date_color;
                i++;

              
                
            }
            sha256obj.Dispose();

            dataGridView1.Refresh();
            dataGridView1.ClearSelection();
        }


        private void Init_GridView()
        {
            dataGridView1.BackgroundColor = Color.FromArgb(28, 30, 32);
            dataGridView1.ScrollBars = ScrollBars.Vertical;
            

            dataGridView1.RowTemplate.Height = 35;

            //File Name Column
            dataGridView1.Columns.Add("time", "Time");
            dataGridView1.Columns["time"].Width = 220;
          

            //File Name Column
            dataGridView1.Columns.Add("filename", "File Name");
            dataGridView1.Columns["filename"].Width = 550;


            //Scan Result Column
            dataGridView1.Columns.Add("result", "Scan Result");
            dataGridView1.Columns["result"].Width = 150;


            //Report Column
            DataGridViewImageColumn img = new DataGridViewImageColumn();
            img.ImageLayout = DataGridViewImageCellLayout.Stretch;
            img.Width = 35;
            dataGridView1.Columns.Add(img);
            img.Image = Properties.Resources.search_black_background;

        }


        private void History_Load(object sender, EventArgs e)
        {
            Init_GridView();
            Init_Filters();
           
        }

        private void Init_Filters()
        {
            int textboxpadding = 8;
            richTextBox1.SetInnerMargins(textboxpadding, 0, textboxpadding, 0);
            richTextBox1.Text = "";
            comboBox1.SelectedIndex = 0;
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = " ";
            dateTimePicker1.Tag = "NoDate";


            dateTimePicker1.Refresh();
        }

        public void ApplyFilters()
        {

            try
            {
                dataGridView1.CurrentCell = null;
                dataGridView1.Rows[0].Visible = false;

                string combo = comboBox1.Text;
                string time = dateTimePicker1.Value.ToString("dd.MM.yyyy");
                string richtextvalue = richTextBox1.Text.ToLower();

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {

                    if( (dateTimePicker1.Tag.Equals("YesDate") && !dataGridView1.Rows[i].Cells[0].Value.ToString().Contains(time))
                        || (!String.IsNullOrWhiteSpace(richTextBox1.Text) && !dataGridView1.Rows[i].Cells[1].Value.ToString().ToLower().Contains(richtextvalue))
                        || (comboBox1.SelectedIndex != 0 && !dataGridView1.Rows[i].Cells[2].Value.ToString().Equals(combo)))
                                 dataGridView1.Rows[i].Visible = false;
                    else
                                 dataGridView1.Rows[i].Visible = true;

                }
            }
            catch {}
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            e.CellStyle.BackColor = Color.FromArgb(28, 30, 32);
        }

        public void SetFileRemovedInRow(int rowid)
        {
            string filename = dataGridView1.Rows[rowid].Cells[1].Value.ToString();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[1].Value.ToString().Equals(filename))
                {
                    dataGridView1.Rows[i].Cells[2].Value = "Removed";
                    dataGridView1.Rows[i].Cells[2].Style.ForeColor = label_color;
                }
            }

           
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 3)
            {
                string value = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value.ToString();
                if (value.Equals("Detected") || value.Equals("Removed"))
                {
                    ReportForm tmp;
                    foreach (Form f in Application.OpenForms)
                    {
               
                        if (f.Tag != null && f.Tag.Equals("ReportForm"))
                        {
                            tmp = (ReportForm)f;
                            if (tmp.reportid == e.RowIndex)
                            {
                                if (f.WindowState == FormWindowState.Minimized)
                                    f.WindowState = FormWindowState.Normal;
                               if(!f.Focused)
                                f.Focus();
                                return;
                            }
                        }
                    }
                    ReportForm reportform = new ReportForm(e.RowIndex, this, value);
                    reportform.Text = "Report for " + Path.GetFileName(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex - 2].Value.ToString());
                    reportform.Tag = "ReportForm";
                    reportform.Show();
                
                }
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void DataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 3)
                dataGridView1.Cursor = Cursors.Default;
            else
            {
                string value = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value.ToString();
                if (value.Equals("Detected") || value.Equals("Removed"))
                    dataGridView1.Cursor = Cursors.Hand;
                else
                    dataGridView1.Cursor = Cursors.Default;
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if(dateTimePicker1.Tag.ToString().Equals("NoDate"))
            {
                dateTimePicker1.CustomFormat = "  dd MMMM yyyy";
                dateTimePicker1.Tag = "YesDate";
                label10.ForeColor = date_color;
                label10.Text = "Reset";
            }
            ApplyFilters();
            this.Focus();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            parentform.FocusHistory();
            ApplyFilters();
        }

        private void label10_Click(object sender, EventArgs e)
        {
            if (dateTimePicker1.Tag.ToString().Equals("YesDate"))
            {
                dateTimePicker1.CustomFormat = " ";
                dateTimePicker1.Tag = "NoDate";
                label10.ForeColor = label_color;
                label10.Text = "Date";
                ApplyFilters();
            }
            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TotalMessage msg = new TotalMessage("Clear History", "Are you sure you want to clear the history?", MessageBoxButtons.YesNo);
            DialogResult res = msg.ShowDialog();
            if(res == DialogResult.Yes)
            {
                if (File.Exists(Settings.ReportsFile))
                    File.Delete(Settings.ReportsFile);
                parentform.last_history = 1;
                parentform.btn_History_Click(null, null);
                
            }
           
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TotalMessage msg = new TotalMessage("Delete Threats", "Are you sure you want to delete all threats?", MessageBoxButtons.YesNo);
            DialogResult res = msg.ShowDialog();
            if (res == DialogResult.Yes)
            {
                string file;
                bool not_removed = false;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[2].Value.ToString().Equals("Detected"))
                    {
                        file = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        try
                        {
                            
                            if (File.Exists(file))
                                 File.Delete(file);
                            
                               
                            dataGridView1.Rows[i].Cells[2].Value = "Removed";
                            dataGridView1.Rows[i].Cells[2].Style.ForeColor = label_color;
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
                                not_removed = true;
                            }
                        }
                        
                    }

                }

                if(not_removed)
                {
                    TotalMessage not_removed_msg = new TotalMessage("Delete Threats Error", "Something went wrong, could not delete some file(s).", MessageBoxButtons.OK);
                    not_removed_msg.ShowDialog();
                }
            }
        }

        
    }
}
