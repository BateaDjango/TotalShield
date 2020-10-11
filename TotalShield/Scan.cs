using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalShield
{
    public partial class Scan : Form
    {
        MainMenu parent = null;
        DateTime start_time;
        bool queue_enabled = false;

        public TotalMessage net_msg = null;
        RunSafeNotify notify_form = null;
        TotalMessage openedmsg = null;
        TotalMessage msg_already_running = null;
        Dictionary<string, bool> tasks = new Dictionary<string, bool>();

        int numberOfFrames = 0;
        int currentFrame = 0;

        readonly Color labelcolor = Color.FromArgb(126, 126, 126);
        readonly Color redlabelcolor = Color.FromArgb(192, 0, 0);

        List<string> upload_files = new List<string>();
        public Scan(MainMenu theparent)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            parent = theparent;
            InitializeComponent();
            timer1.Enabled = false;
        }

        

        private void PictureBox2_Click(object sender, EventArgs e)
        {

            if (!IsAnyKeyValid())
                return;

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories);
                    upload_files = files.ToList();
                    UploadFiles();

                }
            }
        }

        

        private void Scan_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void Scan_DragDrop(object sender, DragEventArgs e)
        {
            if (!IsAnyKeyValid())
                return;


            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                if (File.GetAttributes(file).HasFlag(FileAttributes.Directory))
                    return;
            }

            upload_files = files.ToList();
            UploadFiles();

        }

        public void ResetScan()
        {
            panel1.SendToBack();
            panel1.Visible = false;
        }

        private bool IsAnyKeyValid()
        {
            if (!Settings.isPremium() && !Settings.isPublic())
            {
                TotalMessage msg = new TotalMessage("Scan Error", "No available key found!", MessageBoxButtons.OK);
                msg.ShowDialog();
                msg.Dispose();
                return false;
            }

            return true;
        }

        public async Task<bool> CheckForInternetConnection()
        {
            bool res = await Settings.IsInternetAvailable();
            if (!res)
            {
                if (net_msg != null)
                {
                    net_msg.Close();
                }
                net_msg = new TotalMessage("Scan Error", "No internet connection :( ", MessageBoxButtons.OK);
                net_msg.ShowDialog();
                net_msg = null;

                return false;
            }
            return true;
        }

        private async void UploadFiles()
        {
            bool res = await CheckForInternetConnection();
            if (!res)
                return;


            TogglePanel(panel1, true);
            TogglePanel(panel2, false);

            int files_num = upload_files.Count;
            string plural = (files_num == 1) ? "" : "s";
            label5.Text = files_num.ToString() + " File" + plural + " Uploaded!";
            pictureBox3.Image = Properties.Resources.upload_end;
            pictureBox3.Enabled = true;
            FrameDimension dimension = new FrameDimension(this.pictureBox1.Image.FrameDimensionsList[0]);
            numberOfFrames = this.pictureBox3.Image.GetFrameCount(dimension);
            currentFrame = 0;
        }


        private bool TaskCanceled(string taskid)
        {
            if (tasks[taskid])
            {
                tasks.Remove(taskid);
                return true;
            }

            return false;
        }

        public int getScanProgressValue()
        {
            if (progressBar1.Tag == null || (int)progressBar1.Tag == -1)
                return -1;
            else
                return progressBar1.Value;
        }

        private async Task<bool> ScanFiles(string taskid, bool runsafe)
        {

            Keys keys = Settings.GetKeys();

            bool PremiumAvailable = keys.key[1].value != null && !String.IsNullOrEmpty(keys.key[1].value);
            bool PublicAvailable = keys.key[0].value != null && !String.IsNullOrEmpty(keys.key[0].value);
            bool PremiumActive = keys.key[1].active;
            bool PublicActive = keys.key[0].active;


            string key;
            int scannedfiles = 0;
            int totalfiles = upload_files.Count;
            int threatsfound = 0;
            bool ispremium = PremiumAvailable && PremiumActive;

            if (ispremium)
            {
                key = keys.key[1].value;
            }
            else
            {
                if (PublicAvailable && PublicActive)
                {
                    key = keys.key[0].value;
                }
                else
                {
                    return false;
                }
            }

            List<string> active_avs = Settings.GetActiveAVs();
            AV_Reports av_reports;
            try
            {

                av_reports = (AV_Reports)Settings.Get_Settings(typeof(AV_Reports));
            }
            catch
            {
                av_reports = new AV_Reports();
                av_reports.av_reports = new List<AV_Report>();
            }

            List<string> queued_files = new List<string>();

            if (totalfiles == 1)
                UpdateCurrentFile(upload_files[0]);


            pictureBox4.Image = Properties.Resources.loading;
            pictureBox4.Enabled = true;


            timer1.Enabled = true;
            timer1.Stop();
            timer1.Interval = 1000;
            start_time = DateTime.Now;
            timer1.Start();
            timer1_Tick(null, null);


            foreach (var file in upload_files)
            {
                await Task.Delay(1000);


                AV_Report report = await VT_API.ScanFile(file, key, active_avs, false, ispremium, this);

                if (TaskCanceled(taskid))
                    return false;


                if (report == null)
                {
                    queued_files.Add(file);


                }
                else
                {

                    UpdateCurrentFile(file);
                    av_reports.av_reports.Insert(0, report);
                    scannedfiles++;
                    if (VT_API.IsThreat(report))
                        threatsfound++;
                    UpdateLabels(scannedfiles, totalfiles, threatsfound);


                }
            }

            foreach (var file in queued_files)
            {
                UpdateCurrentFile(file);

                AV_Report report = await VT_API.ScanFile(file, key, active_avs, true, ispremium, this);
                if (TaskCanceled(taskid))
                    return false;




                av_reports.av_reports.Insert(0, report);
                scannedfiles++;
                if (VT_API.IsThreat(report))
                    threatsfound++;
                UpdateLabels(scannedfiles, totalfiles, threatsfound);
            }



            Settings.Save_Settings(av_reports);
            tasks.Remove(taskid);
            timer1.Stop();
            timer1.Enabled = false;
            TogglePanel(panel1, false);
            TogglePanel(panel2, false);

            progressBar1.Tag = -1;

            FilesNum fileinfo = new FilesNum(totalfiles);
            fileinfo.threats = threatsfound;
            fileinfo.duration = "0";
            fileinfo.duration = label15.Text;

            parent.last_history = 1;
            parent.btn_History_Click(fileinfo, null);

            if (openedmsg != null)
            {
                openedmsg.Close();
                openedmsg = null;
            }

            if (runsafe)
            {
                if (threatsfound > 0)
                {
                    notify_form.SetLabelMsg("Threats Found, no file(s) opened.", Color.FromArgb(192, 0, 0));
                    notify_form.ToggleButton(true);
                }
                else
                {
                    await Task.Run(() => StartFiles());

                    notify_form.SetLabelMsg("No Threats Found. Opening File(s)", Color.Green);
                    notify_form.DelayedClose(3000);

                }

            }

            return true;
        }

        private void StartFiles()
        {

            foreach (var file in upload_files)
            {
                try
                {
                    System.Diagnostics.Process.Start(file);
                }
                catch
                {
                    continue;
                }
            }
        }

        private void UpdateCurrentFile(string currentfile)
        {
            label8.Text = currentfile;
        }

        private void UpdateLabels(int scannedfiles, int totalfiles, int threatsfound)
        {
            label10.Text = scannedfiles.ToString();

            if (threatsfound > 0)
                label14.ForeColor = redlabelcolor;
            else
                label14.ForeColor = labelcolor;

            label14.Text = threatsfound.ToString();

            int percent = 100 / totalfiles;

            progressBar1.Value = (progressBar1.Value + percent);
            if (progressBar1.Value == 99)
                progressBar1.Value = 100;
        }


        private void Scan_Load(object sender, EventArgs e)
        {
            TogglePanel(panel1, false);
            TogglePanel(panel2, false);
        }

        

        public void TogglePanel(Panel panel, bool show)
        {
            if (show)
                panel.BringToFront();
            else
                panel.SendToBack();
            panel.Visible = show;
        }

        private async void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (currentFrame == numberOfFrames - 2)
            {
                this.pictureBox3.Enabled = false;
                currentFrame++;
                await Task.Delay(1500);
                Initialize_Scan(false);
            }
            currentFrame++;
        }

        private bool AnyTaskActive()
        {
            foreach (var key in tasks.Keys.ToList())
            {
                if (!tasks[key])
                    return true;
            }

            return false;
        }

        public void AddToScan(string file, int scan_cmd)
        {
            if (AnyTaskActive())
            {
                if (msg_already_running == null)
                {
                    TotalMessage msg = new TotalMessage("Files Scan", "There is already an active scan running", MessageBoxButtons.OK);
                    msg_already_running = msg;
                    msg.ShowDialog();
                    msg.Dispose();
                    msg_already_running = null;
                    return;
                }
            }

            if (!IsAnyKeyValid())
                return;

            if (!queue_enabled)
            {
                if (scan_cmd == 1 && parent.Visible && parent.opened_runsafe)
                {
                    parent.HideForm();
                    parent.opened_runsafe = false;

                }


                queue_enabled = true;
                Task.Run(() => CloseQueue((scan_cmd == 1) ? true : false));
                upload_files.Clear();
            }

            if (queue_enabled)
            {

                upload_files.Add(file);

            }
        }

        public async void CloseQueue(bool runsafe)
        {
            await Task.Delay(500);
            queue_enabled = false;
            this.Invoke(new Action(() => Initialize_Scan(runsafe)));
        }

        public async void Initialize_Scan(bool runsafe)
        {
            if (AnyTaskActive())
                return;

            bool res = await CheckForInternetConnection();

            if (!res)
                return;

            if (!runsafe)
            {
                if (parent.ShowInTaskbar == false)
                {
                    parent.ShowForm();
                }
                else
                {
                    if (parent.WindowState == FormWindowState.Minimized)
                        parent.WindowState = FormWindowState.Normal;
                }
            }



            TogglePanel(panel1, false);
            TogglePanel(panel2, true);
            label8.Text = "";
            label10.Text = "0";
            label12.Text = upload_files.Count.ToString();
            label14.Text = "0";
            progressBar1.Value = 0;
            progressBar1.Tag = 0;

            string uid = Guid.NewGuid().ToString();
            tasks.Add(uid, false);

            if (runsafe)
            {
                if (notify_form != null)
                {
                    notify_form.Close();
                    notify_form.Dispose();
                }

                notify_form = new RunSafeNotify(parent);
                notify_form.Show();
            }

            await ScanFiles(uid, runsafe);


        }


        private void button1_Click(object sender, EventArgs e)
        {
            TotalMessage msg = new TotalMessage("Files Scan", "Are you sure you want to cancel the scan?", MessageBoxButtons.YesNo);
            openedmsg = msg;
            if (msg.ShowDialog() == DialogResult.Yes)
            {
                foreach (var key in tasks.Keys.ToList())
                {
                    tasks[key] = true;
                }

                timer1.Stop();
                timer1.Enabled = false;
                progressBar1.Tag = -1;
                TogglePanel(panel1, false);
                TogglePanel(panel2, false);
            }
        }

        

        

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - start_time;
            label15.Text = ts.ToString(@"hh\:mm\:ss");
        }
    }
}
