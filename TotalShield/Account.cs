using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirusTotalNet;
using VirusTotalNet.Results;

namespace TotalShield
{
    public partial class Account : Form
    {
        int ok_total = 0;
        int ok_current = 0;
        int nok_total = 0;
        int nok_current = 0;


        int picturebox3isok = 0;
        int picturebox2isok = 0;


        System.Timers.Timer tutorial_timer = null;



        public Account()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
           
        }

        public void LoadKeys()
        {
            Keys keys = Settings.GetKeys();

            string str, passchars; int middle;
            str = keys.key[0].value;

            if (!String.IsNullOrWhiteSpace(str))
            {

                middle = str.Length / 3;
                passchars = new String('*', str.Length - middle);
                str = str.Remove(middle).Insert(middle, passchars);
                richTextBox1.Text = str;
            }
            else
                richTextBox1.Text = String.Empty;

            str = keys.key[1].value;

            if (!String.IsNullOrWhiteSpace(str))
            {
                str = richTextBox2.Text.ToString();
                middle = str.Length / 3;
                passchars = new String('*', str.Length - middle);
                str = str.Remove(middle).Insert(middle, passchars);
                richTextBox2.Text = str;
            }
            else
                richTextBox2.Text = String.Empty;
            richTextBox1.SelectionStart = 0;
            richTextBox1.SelectionLength = 0;
            richTextBox1.Focus();
            richTextBox2.SelectionStart = 0;
            richTextBox2.SelectionLength = 0;
            richTextBox2.Focus();

        }

        private void Account_Load(object sender, EventArgs e)
        {
           

            int textboxpadding = 8;
            richTextBox1.SetInnerMargins(textboxpadding, 0, textboxpadding, 0);
            richTextBox2.SetInnerMargins(textboxpadding, 0, textboxpadding, 0);


        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            Fix_Textbox_Format(richTextBox1);
            pictureBox2.Image = null;

        }

        private void Fix_Textbox_Format(RichTextBox target)
        {
            int start = target.SelectionStart;
            string tmp = target.Text.Replace(" ", "");
            target.Text = tmp;
            target.SelectionStart = start;
            target.SelectionLength = 0;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            Fix_Textbox_Format(richTextBox2);
            pictureBox3.Image = null;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = null;
            bool is_key_valid = await VT_API.IsKeyValid(richTextBox1.Text, false);

            if (is_key_valid)
            {
                StartOK(pictureBox2);
                Settings.SaveKey(richTextBox1.Text, false);

            }
            else
            {
                StartNOK(pictureBox2);
            }


        }

        

        

        public void StartOK(PictureBox pic)
        {
            pic.Enabled = true;
            picturebox2isok = 0;
            picturebox3isok = 0;
            if (pic.Name.Contains("2"))
            {
                picturebox2isok = 1;
            }
            else
            {
                picturebox3isok = 1;
            }

            pic.Image = Properties.Resources.ok1;
            FrameDimension dimension = new FrameDimension(pic.Image.FrameDimensionsList[0]);
            ok_total = pic.Image.GetFrameCount(dimension);
            ok_current = 0;
          
        }

        public void StartNOK(PictureBox pic)
        {
            pic.Enabled = true;
            picturebox2isok = 0;
            picturebox3isok = 0;
            if (pic.Name.Contains("3"))
            {
                picturebox3isok = 2;
            }
            else
            {
                picturebox2isok = 2;
            }
            
            pic.Image = Properties.Resources.nok1;
            FrameDimension dimension = new FrameDimension(pic.Image.FrameDimensionsList[0]);
            nok_total = pic.Image.GetFrameCount(dimension);
            nok_current = 0;
           
        }

        private async void button2_ClickAsync(object sender, EventArgs e)
        {
            pictureBox3.Image = null;
            bool is_key_valid = await VT_API.IsKeyValid(richTextBox2.Text, true);

            if (is_key_valid)
            {
                StartOK(pictureBox3);
                Settings.SaveKey(richTextBox2.Text, true);
            }
            else
            {
                StartNOK(pictureBox3);
            }
        }


        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            
            
            if(picturebox2isok == 1)
            {

                if (ok_current == ok_total-5)
                {
                    this.pictureBox2.Image = Properties.Resources.ok2;
                    
                }
                ok_current++;

            }
            else
            {
                if (picturebox2isok == 2)
                {
                    if (nok_current == nok_total-5)
                    {
                        this.pictureBox2.Image = Properties.Resources.nok2;

                    }
                    nok_current++;
                }
                
            }
            

        }


        public void RefreshPictureboxes()
        {
            RefreshPic(pictureBox2, picturebox2isok);
            RefreshPic(pictureBox3, picturebox3isok);
        }

        private void RefreshPic(PictureBox pic, int isok)
        {
            if (isok == 0)
                return;

            if (isok == 1)
            {
                pic.Image = Properties.Resources.ok2;
            }
            else
            {
               
               
               pic.Image = Properties.Resources.nok2;
                
            }
        }


        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
           
            if (picturebox3isok == 1)
            {

                if (ok_current == ok_total-5)
                {
                    this.pictureBox3.Image = Properties.Resources.ok2;

                }
                ok_current++;

            }
            else
            {
                if (picturebox3isok == 2)
                {
                    if (nok_current == nok_total-5)
                    {
                        this.pictureBox3.Image = Properties.Resources.nok2;

                    }
                    nok_current++;
                }
             
            }
        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.BringToFront();
            button1.Visible = false;
            button2.Visible = false;
            pictureBox4.Image = Properties.Resources.tutorial;
            this.pictureBox4.Enabled = true;

            if(tutorial_timer != null)
            tutorial_timer.Stop();
           

            tutorial_timer = new System.Timers.Timer(30000);
           
            tutorial_timer.Elapsed -= StopTutorial;
            tutorial_timer.Elapsed += StopTutorial;

            tutorial_timer.Start();

        }

        private void StopTutorial(object sender, EventArgs e)
        {
            tutorial_timer.Stop();
            pictureBox4.Enabled = false;
      
        }



        private void label2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.SendToBack();
            button1.Visible = true;
            button2.Visible = true;
            pictureBox4.Image = null;
        }

        
       
      
    }
}
