using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using AutoPrint.Properties;

namespace AutoPrint
{
    public partial class preference : Form
    {
        public preference()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Settings.Default.WindowLocation = this.Location;

            // Copy window size to app settings
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowSize = this.Size;
            }
            else
            {
                Settings.Default.WindowSize = this.RestoreBounds.Size;
            }

            // Save settings
            Settings.Default.Save();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        public void ChooseFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    textBox1.Text = fbd.SelectedPath;
                    System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                }
            }
        }

        private void preference_Load(object sender, EventArgs e)
        {
            if (!Settings.Default.folderpath.Equals("")) {
                textBox1.Text = Settings.Default.folderpath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Settings.Default.folderpath = textBox1.Text.ToString();
            this.Close();
        }
    }
}
