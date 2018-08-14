using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using AutoPrint.Properties;
using System.IO;
using System.Threading;
using System.Drawing.Printing;
using MySql.Data.MySqlClient;


namespace AutoPrint
{
    public partial class AutoPrint : Form
    {

        delegate void SetTextCallback(string text);

        int Timercount = 500;

        System.Windows.Forms.Timer aTimer;

        private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupValue = "AutoPrint";

        private static int connectionstatus = 0;

        private String FilePath;
        private int count= 0;

        private Label labeldata;

        private Thread demoThread = null;

        FileSystemWatcher watcher=null;

        private PrintDocument printDocument = new PrintDocument();
        private String stringToPrint = "";

        public AutoPrint()
        {
            
            InitializeComponent();

            executeclick();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Settings.Default.Startup.Equals("0"))
            {
                SetStartup();
                MessageBox.Show("Added to Startup", "Activated");
                Settings.Default.Startup = "1";
                button1.Text = "Enabled";
            }
            else {
                SetStartup();
                MessageBox.Show("Removed from Startup", "Deactivated");
                Settings.Default.Startup = "0";
                button1.Text = "Disabled";
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            if (Settings.Default.WindowLocation != null)
            {
                this.Location = Settings.Default.WindowLocation;
            }

            if (Settings.Default.Startup != null) {
                if (Settings.Default.Startup == "0")
                {
                    button1.Text = "Disabled";
                }
                else {
                    button1.Text = "Enabled";
                }
            }

            // Set window size
            if (Settings.Default.WindowSize != null)
            {
                this.Size = Settings.Default.WindowSize;
            }
           
            
            /*watcher = new FileSystemWatcher();
            watcher.Path = Settings.Default.folderpath;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.png";
            watcher.Changed += new FileSystemEventHandler(OnChanged);*/
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

      /*  private void button3_Click(object sender, EventArgs e)
        {
            watch(label1);
        }*/

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.IO.Path.Combine(Environment.SystemDirectory, "control.exe"), "/name Microsoft.DevicesAndPrinters");
        }
        private static void SetStartup()
        {
            //Set the application to run at startup
            RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
            key.SetValue(StartupValue, Application.ExecutablePath.ToString());
        }
        private static void unSetStartup()
        {
            //Set the application to run at startup
            RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
            key.DeleteValue(Application.ExecutablePath.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form sett = new preference();
            sett.ShowDialog();
            
        }

      /*  private void watch(Label data)
        {
            labeldata = data;
            try {
                if (button3.Text.ToString().Equals("Started"))
                {
                    watcher.EnableRaisingEvents = false;
                    label1.Text = "Stopped";

                    button3.Text = "Stopped";
                }
                else {
                    if (Settings.Default.folderpath == null || Settings.Default.folderpath.Equals(""))
                    {
                        MessageBox.Show("Empty Folder Path", "Error");

                        Form sett = new preference();
                        sett.ShowDialog();

                        button3.Text = "Stopped";
                    }
                    else {

                        
                        watcher.EnableRaisingEvents = true;
                        button3.Text = "Started";
                        label1.Text = "Idle";
                    }
                }
                
            }
            catch (Exception e) {

                button3.Text = "Error Occured";
                MessageBox.Show(e.ToString() , "Error");

            }
            

        }*/

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
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
        }
        private void WaitForFile(FileInfo file,FileSystemEventArgs e)
        {
            FileStream stream = null;
            bool FileReady = false;
            while (!FileReady)
            {
                try
                {
                    using (stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        FileReady = true;
                        try
                        {
                            if (FileReady)
                            {
                                
                            }

                        }
                        catch (Exception excep)
                        {
                            MessageBox.Show(excep.ToString() + " " + e.FullPath, "Error");
                            File.Delete(e.FullPath);
                        }
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("File is not Ready", "Error");
                    //File isn't ready yet, so we need to keep on waiting until it is.
                }
                catch (OutOfMemoryException) {

                }
                //We'll want to wait a bit between polls, if the file isn't ready.
            }
        }

        

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public void updatelabel1(String Data) {
            label1.Text = Data;
        }


        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
            }
        }

        private void ThreadProcSafe()
        {
            this.SetText("Printing...");
        }

        private void cleaning()
        {
            this.SetText("Cleaning...");
        }

        private void itemleft(String data)
        {
            this.SetText(data);
        }

        private void idle()
        {
            this.SetText("idle");
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            count++;

            this.demoThread =
                new Thread(new ThreadStart(this.ThreadProcSafe));

            this.demoThread.Start();

            FileInfo file = new FileInfo(e.FullPath);
            Thread.Sleep(2000);

            try
            {

                using (PrintDocument myDoc = new PrintDocument())
                {
                    myDoc.PrintPage += new PrintPageEventHandler(print);
                    FilePath = e.FullPath;
                    PrinterSettings settings = new PrinterSettings();
                    myDoc.PrinterSettings.PrinterName = settings.PrinterName;
                    myDoc.Print();
                    this.demoThread =
                    new Thread(new ThreadStart(this.cleaning));

                    this.demoThread.Start();
                    
                    while (IsFileLocked(file))
                    {
                        if (!IsFileLocked(file)) {
                            File.Delete(e.FullPath);
                            break;
                        }
                        Thread.Sleep(1000);
                    }

                    this.demoThread =
                    new Thread(new ThreadStart(this.idle));

                    this.demoThread.Start();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show("Error " + err.ToString(), "Error");
            }

            //   if (count > 3) {
            //  }
            //Copies file to another directory.
        }

        private void print(object sender, PrintPageEventArgs e)
        {
            try
            {
                Image data = (Image)ResizeImage(Image.FromFile(FilePath), 270, 600);
                using (Image i = data)
                {
                    Point p = new Point(0, 0);
                    e.Graphics.DrawImage(i, p);
                }
            }
            catch (Exception exep)
            {
                throw exep;
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (connectionstatus == 0)
            {

                if (Settings.Default.MYSQLDatabase != null || Settings.Default.MYSQLDatabase != "" && Settings.Default.MYSQLServer != null || Settings.Default.MYSQLServer != "" && Settings.Default.MYSQLUsername != null || Settings.Default.MYSQLUsername != "")
                {

                    String Temp = "";

                    if (Settings.Default.MYSQLPassword == null)
                    {

                    }
                    else
                    {
                        Temp = Settings.Default.MYSQLPassword;
                    }

                    if (mysqlform.checkconn(Settings.Default.MYSQLServer, 3306, Settings.Default.MYSQLDatabase, Settings.Default.MYSQLUsername, Temp))
                    {
                        MessageBox.Show("Connected", "Info");
                        connectionstatus = 1;
                        label1.Text = "Monitoring";
                        label2.Text = "Connected";
                        button5.Text = "Connected";

                        aTimer = new System.Windows.Forms.Timer();
                        aTimer.Tick += timer_Tick;
                        aTimer.Start();
                        aTimer.Interval = Timercount;
                        //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                        //aTimer.Interval = 5000;
                        //aTimer.Enabled = true;

                    }
                    else {
                        MessageBox.Show("Error Occured", "Info");
                        label2.Text = "Disconnected";
                        button5.Text = "Disconnected";
                        label1.Text = "Not Monitoring";
                    }
                }
                else {
                    mysqlform forms = new mysqlform();
                    forms.ShowDialog();
                }
            }
            else {
                aTimer.Stop();
                connectionstatus = 0;
                label2.Text = "Disconnected";
                label1.Text = "Not Monitoring";
                button5.Text = "Disconnected";
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            String Temp = "";

            if (Settings.Default.MYSQLPassword == null)
            {

            }
            else
            {
                Temp = Settings.Default.MYSQLPassword;
            }

            var conn_info = "Server=" + Settings.Default.MYSQLServer + ";Port=" + 3306 + ";Database=" + Settings.Default.MYSQLDatabase + ";Uid=" + Settings.Default.MYSQLUsername + ";Pwd=" + Temp + ";SslMode=none";
            bool isConn = false;
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(conn_info);
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT distinct(order_number),print from checkout where print!='1' ";


                MySqlDataReader reader = cmd.ExecuteReader();

                int counting = 0;
                int countreprint = 0;
                List<String> data = new List<String>();

                while (reader.Read())
                {

                    if (reader.GetInt32("print")==0) {
                        counting++;
                    }
                    if (reader.GetInt32("print") == 2) {
                        countreprint++;
                    }
                    
                    //reading += reader.GetString("KODE_STOCK") + " ";
                }
                reader.Close();
                conn.Close();

                if (counting != 0) {
                    printReceipt();
                    printneworder();
                    printnumber();
                    clearprint("update checkout set print=1 where order_number=(select * from (SELECT distinct(order_number) from checkout where print='0' limit 1) t)", conn);
                }

                if (countreprint != 0)
                {
                    reprintReceipt();
                    clearreprint("update checkout set print=1 where order_number=(select * from (SELECT distinct(order_number) from checkout where print='2' limit 1) t)", conn);
                }

                label1.Text = counting+countreprint + " Items Left";

                
               

                conn.Close();
            }
            catch (Exception excepe)
            {
                MessageBox.Show(excepe.ToString(), "Error Query");
                aTimer.Enabled = false;
            }
        }
        public void clearreprint(string myExecuteQuery, MySqlConnection myConnection)
        {
            myConnection.Open();
            MySqlCommand myCommand = new MySqlCommand(myExecuteQuery, myConnection);
            myCommand.ExecuteNonQuery();
            myConnection.Close();
        }
        public void clearprint (string query, MySqlConnection myConnection)
        {
            myConnection.Open();
            MySqlCommand myCommand = new MySqlCommand(query, myConnection);
            myCommand.ExecuteNonQuery();
            myConnection.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            mysqlform form = new mysqlform();
            form.Show();
        }
        
        private void label2_Click(object sender, EventArgs e)
        {

        }
        

        private void printPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;

            Font regular = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Regular);
            Font bold = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Bold);
            Font smaller = new Font(FontFamily.GenericSansSerif, 8.0f, FontStyle.Regular);
            Font bigger = new Font(FontFamily.GenericSansSerif, 11.0f, FontStyle.Regular);
            Font bigbold = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Bold);

            //print header
            graphics.DrawString("Let's Happy Bellying!", regular, Brushes.Black, 70, 10);
            graphics.DrawString("Frying now @ Brastagi Tiara", regular, Brushes.Black, 50, 30);
            graphics.DrawString("Operation Hours(Daily):", regular, Brushes.Black, 70, 50);
            graphics.DrawString("10:00 a.m - 10:00 p.m", regular, Brushes.Black, 70,70);
            graphics.DrawLine(Pens.Black, 20, 110, 260, 110);
            graphics.DrawLine(Pens.Black, 20, 115, 260, 115);


            //print items
            //graphics.DrawString("COD | DESCRICAO                      | QTY | X | Vir Unit | Vir Total |", bold, Brushes.Black, 10, 120);
            //graphics.DrawLine(Pens.Black, 10, 140, 430, 140);

            String Temp = "";

            if (Settings.Default.MYSQLPassword == null)
            {

            }
            else
            {
                Temp = Settings.Default.MYSQLPassword;
            }

            var conn_info = "Server=" + Settings.Default.MYSQLServer + ";Port=" + 3306 + ";Database=" + Settings.Default.MYSQLDatabase + ";Uid=" + Settings.Default.MYSQLUsername + ";Pwd=" + Temp + ";SslMode=none";
            bool isConn = false;
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(conn_info);
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT distinct(order_number) from checkout where print='0' limit 1 ";


                MySqlDataReader reader = cmd.ExecuteReader();

                String nofaktur = "";
                while (reader.Read())
                {
                    nofaktur = reader.GetString("order_number");

                }

                reader.Close();

                MySqlCommand cmdiappenjualan = conn.CreateCommand();
                cmdiappenjualan.CommandText = "SELECT * from iappenjualan where no_faktur='" + nofaktur +"'";
                MySqlDataReader iappenjualan = cmdiappenjualan.ExecuteReader();

                Double totalall = 0.0d;

                while (iappenjualan.Read())
                {

                    graphics.DrawString("Order NO : " + iappenjualan.GetString("NO_FAKTUR"), smaller, Brushes.Black, 10, 125);
                    graphics.DrawString("Date : "+ iappenjualan.GetDateTime("TANGGAL").ToString("dd/MM/yyyy  hh:mm tt"), smaller, Brushes.Black, 10, 145);
                    graphics.DrawString("Transaction By : "+ iappenjualan.GetString("USER_ID"), smaller, Brushes.Black, 10, 165);
                    graphics.DrawString("NO   Description                                         Amt(due)  ", smaller, Brushes.Black, 10, 195);
                    totalall = iappenjualan.GetDouble("JUMLAH_FAKTUR_RP");

                }
                iappenjualan.Close();

                MySqlCommand cmdiatpenjualan1 = conn.CreateCommand();
                cmdiatpenjualan1.CommandText = "SELECT * from iatpenjualan1 where no_faktur='" + nofaktur + "' order by no_item asc";
                MySqlDataReader iatpenjualan1 = cmdiatpenjualan1.ExecuteReader();

                int currentgap = 195;
                int gap = 15;
                int desclength = " Description1231233333333333333333333333333131".Length;
                int numlength = "NO  ".Length;
                int amtlength = "1231233333331231231".Length;
                int extra = 0;
                int count = 0;
                String tempstringagain = "";

                while (iatpenjualan1.Read())
                {
                    
                    String tempno = "";
                    String tempdesc = "";
                    String extradesc = "";
                    String tempamt = "";

                    tempno = iatpenjualan1.GetString("no_item")+".";
                    while (tempno.Length < numlength)
                    {
                        tempno = tempno+" ";
                    }

                    tempdesc = "   " + iatpenjualan1.GetString("nama_stock")+"("+ String.Format("{0:n0}", iatpenjualan1.GetDouble("qty"))+")";

                    while (tempdesc.Length < desclength)
                    {
                        tempdesc = tempdesc + " ";

                    }

                    if (tempdesc.Length>desclength)
                    {
                        extra = 1;
                        tempstringagain = tempdesc;
                        tempdesc = tempdesc.Substring(0, desclength);
                        tempstringagain = tempstringagain.Substring(desclength, tempstringagain.Length-desclength);
                    }

                    //String.Format("{0:n0}", 1234); //Output: 1,234.00

                    //string.Format("{0:n0}", 9876);

                    tempamt = "Rp " + String.Format("{0:n0}",iatpenjualan1.GetDouble("qty") * iatpenjualan1.GetDouble("harga_jual"));
                    while (tempamt.Length < amtlength)
                    {
                        tempamt = " " + tempamt;
                    }

                    graphics.DrawString(tempno+tempdesc, smaller, Brushes.Black, 10, currentgap+gap);
                    graphics.DrawString(tempamt, smaller, Brushes.Black, 170, currentgap + gap);

                    if (extra == 1) {
                        currentgap = currentgap + 8;
                        graphics.DrawString(tempstringagain, smaller, Brushes.Black, 10 + numlength, currentgap + gap);
                        extra = 0;
                    }
                    currentgap = currentgap + gap;
                    //graphics.DrawString("COD | DESCRICAO                      | QTY | X | Vir Unit | Vir Total |", bold, Brushes.Black, 10, 221);
                    count = count + iatpenjualan1.GetInt32("qty");
                }
                iatpenjualan1.Close();

                currentgap = currentgap + 50;

                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);
                currentgap = currentgap + 10;

                int totalsemua = "                    Amt(due)  ".Length;

                String counttotalleng = "Rp "+ String.Format("{0:n0}", totalall);


                while (counttotalleng.Length < totalsemua) {
                    counttotalleng = " " + counttotalleng;
                }
                
                graphics.DrawString("    Subtotal ( " +count +")                    "+counttotalleng, smaller, Brushes.Black, 10, currentgap);

                currentgap = currentgap + 25;
                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);

                cmdiappenjualan = conn.CreateCommand();
                cmdiappenjualan.CommandText = "SELECT * from iappenjualan where no_faktur='" + nofaktur + "'";
                iappenjualan = cmdiappenjualan.ExecuteReader();


                int footerlength = "                        ".Length;

                while (iappenjualan.Read())
                {
                    String totalfooter = "Rp " + String.Format("{0:n0}", iappenjualan.GetDouble("JUMLAH_FAKTUR_RP"));

                    

                    graphics.DrawString("       Total", bigbold, Brushes.Black, 10, currentgap);
                    graphics.DrawString(totalfooter, bigbold, Brushes.Black, 160, currentgap);


                    currentgap = currentgap + 25;

                    String cashfooter = "Rp " + String.Format("{0:n0}", iappenjualan.GetDouble("BAYAR"));

                    

                    graphics.DrawString("       Cash", bigger, Brushes.Black, 10, currentgap);
                    graphics.DrawString(cashfooter, bigger, Brushes.Black, 165, currentgap);

                    currentgap = currentgap + 20;

                    String amtduefooter = "Rp " + String.Format("{0:n0}", iappenjualan.GetDouble("BAYAR")- iappenjualan.GetDouble("JUMLAH_FAKTUR_RP"));

                    

                    graphics.DrawString("       Amt(Due)", bigger, Brushes.Black, 10, currentgap);
                    graphics.DrawString(amtduefooter, bigger, Brushes.Black, 170, currentgap);

                    currentgap = currentgap + 20;
                }

                iappenjualan.Close();

                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Cater for office meeting, event birthday", regular, Brushes.Black, 10, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Please contact us :", regular, Brushes.Black, 70, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("WA : 085922380750", regular, Brushes.Black, 70, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Line : happybellying", regular, Brushes.Black, 60, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Instagram : happybellying", regular, Brushes.Black, 50, currentgap);

                currentgap = currentgap + 20;
                conn.Close();

            }
            catch (Exception exp) {
                MessageBox.Show("ERROR : " + exp.ToString(), "Error");
            }


                

            //for (int i = 0; i < itemList.Count; i++)
            //{
            //    graphics.DrawString(itemList[i].ToString(), regular, Brushes.Black, 20, 150 + i * 20);
            //}

            //print footer
            //...

            regular.Dispose();
            bold.Dispose();

            // Check to see if more pages are to be printed.
            
        }

        private void reprintPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;

            Font regular = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Regular);
            Font bold = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Bold);
            Font smaller = new Font(FontFamily.GenericSansSerif, 8.0f, FontStyle.Regular);
            Font bigger = new Font(FontFamily.GenericSansSerif, 11.0f, FontStyle.Regular);
            Font bigbold = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Bold);

            //print header
            graphics.DrawString("Let's Happy Bellying!", regular, Brushes.Black, 70, 10);
            graphics.DrawString("Frying now @ Brastagi Tiara", regular, Brushes.Black, 50, 30);
            graphics.DrawString("Operation Hours(Daily):", regular, Brushes.Black, 70, 50);
            graphics.DrawString("10:00 a.m - 10:00 p.m", regular, Brushes.Black, 70, 70);
            graphics.DrawLine(Pens.Black, 20, 110, 260, 110);
            graphics.DrawLine(Pens.Black, 20, 115, 260, 115);


            //print items
            //graphics.DrawString("COD | DESCRICAO                      | QTY | X | Vir Unit | Vir Total |", bold, Brushes.Black, 10, 120);
            //graphics.DrawLine(Pens.Black, 10, 140, 430, 140);

            String Temp = "";

            if (Settings.Default.MYSQLPassword == null)
            {

            }
            else
            {
                Temp = Settings.Default.MYSQLPassword;
            }

            var conn_info = "Server=" + Settings.Default.MYSQLServer + ";Port=" + 3306 + ";Database=" + Settings.Default.MYSQLDatabase + ";Uid=" + Settings.Default.MYSQLUsername + ";Pwd=" + Temp + ";SslMode=none";
            bool isConn = false;
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(conn_info);
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT distinct(order_number) from checkout where print='2' limit 1 ";


                MySqlDataReader reader = cmd.ExecuteReader();

                String nofaktur = "";
                while (reader.Read())
                {
                    nofaktur = reader.GetString("order_number");

                }

                reader.Close();

                MySqlCommand cmdiappenjualan = conn.CreateCommand();
                cmdiappenjualan.CommandText = "SELECT * from iappenjualan where no_faktur='" + nofaktur + "'";
                MySqlDataReader iappenjualan = cmdiappenjualan.ExecuteReader();

                Double totalall = 0.0d;

                while (iappenjualan.Read())
                {

                    graphics.DrawString("Order NO : " + iappenjualan.GetString("NO_FAKTUR"), smaller, Brushes.Black, 10, 125);
                    graphics.DrawString("Date : " + iappenjualan.GetDateTime("TANGGAL").ToString("dd/MM/yyyy  hh:mm tt"), smaller, Brushes.Black, 10, 145);
                    graphics.DrawString("Transaction By : " + iappenjualan.GetString("USER_ID"), smaller, Brushes.Black, 10, 165);
                    graphics.DrawString("NO   Description                                         Amt(due)  ", smaller, Brushes.Black, 10, 195);
                    totalall = iappenjualan.GetDouble("JUMLAH_FAKTUR_RP");

                }
                iappenjualan.Close();

                MySqlCommand cmdiatpenjualan1 = conn.CreateCommand();
                cmdiatpenjualan1.CommandText = "SELECT * from iatpenjualan1 where no_faktur='" + nofaktur + "' order by no_item asc";
                MySqlDataReader iatpenjualan1 = cmdiatpenjualan1.ExecuteReader();

                int currentgap = 195;
                int gap = 15;
                int desclength = " Description1231233333333333333333333333333131".Length;
                int numlength = "NO  ".Length;
                int amtlength = "1231233333331231231".Length;
                int extra = 0;
                int count = 0;
                String tempstringagain = "";

                while (iatpenjualan1.Read())
                {

                    String tempno = "";
                    String tempdesc = "";
                    String extradesc = "";
                    String tempamt = "";

                    tempno = iatpenjualan1.GetString("no_item") + ".";
                    while (tempno.Length < numlength)
                    {
                        tempno = tempno + " ";
                    }

                    tempdesc = "   " + iatpenjualan1.GetString("nama_stock") + "(" + String.Format("{0:n0}", iatpenjualan1.GetDouble("qty")) + ")";

                    while (tempdesc.Length < desclength)
                    {
                        tempdesc = tempdesc + " ";

                    }

                    if (tempdesc.Length > desclength)
                    {
                        extra = 1;
                        tempstringagain = tempdesc;
                        tempdesc = tempdesc.Substring(0, desclength);
                        tempstringagain = tempstringagain.Substring(desclength, tempstringagain.Length - desclength);
                    }

                    //String.Format("{0:n0}", 1234); //Output: 1,234.00

                    //string.Format("{0:n0}", 9876);

                    tempamt = "Rp " + String.Format("{0:n0}", iatpenjualan1.GetDouble("qty") * iatpenjualan1.GetDouble("harga_jual"));
                    while (tempamt.Length < amtlength)
                    {
                        tempamt = " " + tempamt;
                    }

                    graphics.DrawString(tempno + tempdesc, smaller, Brushes.Black, 10, currentgap + gap);
                    graphics.DrawString(tempamt, smaller, Brushes.Black, 170, currentgap + gap);

                    if (extra == 1)
                    {
                        currentgap = currentgap + 8;
                        graphics.DrawString(tempstringagain, smaller, Brushes.Black, 10 + numlength, currentgap + gap);
                        extra = 0;
                    }
                    currentgap = currentgap + gap;
                    //graphics.DrawString("COD | DESCRICAO                      | QTY | X | Vir Unit | Vir Total |", bold, Brushes.Black, 10, 221);
                    count = count + iatpenjualan1.GetInt32("qty");
                }
                iatpenjualan1.Close();

                currentgap = currentgap + 50;

                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);
                currentgap = currentgap + 10;

                int totalsemua = "                    Amt(due)  ".Length;

                String counttotalleng = "Rp " + String.Format("{0:n0}", totalall);


                while (counttotalleng.Length < totalsemua)
                {
                    counttotalleng = " " + counttotalleng;
                }

                graphics.DrawString("    Subtotal ( " + count + ")                    " + counttotalleng, smaller, Brushes.Black, 10, currentgap);

                currentgap = currentgap + 25;
                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);

                cmdiappenjualan = conn.CreateCommand();
                cmdiappenjualan.CommandText = "SELECT * from iappenjualan where no_faktur='" + nofaktur + "'";
                iappenjualan = cmdiappenjualan.ExecuteReader();


                int footerlength = "                        ".Length;

                while (iappenjualan.Read())
                {
                    String totalfooter = "Rp " + String.Format("{0:n0}", iappenjualan.GetDouble("JUMLAH_FAKTUR_RP"));



                    graphics.DrawString("       Total", bigbold, Brushes.Black, 10, currentgap);
                    graphics.DrawString(totalfooter, bigbold, Brushes.Black, 160, currentgap);


                    currentgap = currentgap + 25;

                    String cashfooter = "Rp " + String.Format("{0:n0}", iappenjualan.GetDouble("BAYAR"));



                    graphics.DrawString("       Cash", bigger, Brushes.Black, 10, currentgap);
                    graphics.DrawString(cashfooter, bigger, Brushes.Black, 165, currentgap);

                    currentgap = currentgap + 20;

                    String amtduefooter = "Rp " + String.Format("{0:n0}", iappenjualan.GetDouble("BAYAR") - iappenjualan.GetDouble("JUMLAH_FAKTUR_RP"));



                    graphics.DrawString("       Amt(Due)", bigger, Brushes.Black, 10, currentgap);
                    graphics.DrawString(amtduefooter, bigger, Brushes.Black, 170, currentgap);

                    currentgap = currentgap + 20;
                }

                iappenjualan.Close();

                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Cater for office meeting, event birthday", regular, Brushes.Black, 10, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Please contact us :", regular, Brushes.Black, 70, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("WA : 085922380750", regular, Brushes.Black, 70, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Line : happybellying", regular, Brushes.Black, 60, currentgap);

                currentgap = currentgap + 20;

                graphics.DrawString("Instagram : happybellying", regular, Brushes.Black, 50, currentgap);

                currentgap = currentgap + 20;
                conn.Close();

            }
            catch (Exception exp)
            {
                MessageBox.Show("ERROR : " + exp.ToString(), "Error");
            }




            //for (int i = 0; i < itemList.Count; i++)
            //{
            //    graphics.DrawString(itemList[i].ToString(), regular, Brushes.Black, 20, 150 + i * 20);
            //}

            //print footer
            //...

            regular.Dispose();
            bold.Dispose();

            // Check to see if more pages are to be printed.

        }

        private void printReceipt()
        {
            try
            {

                using (PrintDocument myDoc = new PrintDocument())
                {
                    myDoc.PrintPage += new PrintPageEventHandler(printPage);
                    PrinterSettings settings = new PrinterSettings();
                    myDoc.PrinterSettings.PrinterName = settings.PrinterName;
                    myDoc.Print();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show("Error " + err.ToString(), "Error");
            }
        }

        private void reprintReceipt()
        {
            try
            {

                using (PrintDocument myDoc = new PrintDocument())
                {
                    myDoc.PrintPage += new PrintPageEventHandler(reprintPage);
                    PrinterSettings settings = new PrinterSettings();
                    myDoc.PrinterSettings.PrinterName = settings.PrinterName;
                    myDoc.Print();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show("Error " + err.ToString(), "Error");
            }
        }
        private void printneworder()
        {
            try
            {

                using (PrintDocument myDoc = new PrintDocument())
                {
                    myDoc.PrintPage += new PrintPageEventHandler(printPageorder);
                    PrinterSettings settings = new PrinterSettings();
                    myDoc.PrinterSettings.PrinterName = settings.PrinterName;
                    myDoc.Print();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show("Error " + err.ToString(), "Error");
            }
        }
        private void printnumber()
        {
            try
            {

                using (PrintDocument myDoc = new PrintDocument())
                {
                    myDoc.PrintPage += new PrintPageEventHandler(printPagenumber);
                    PrinterSettings settings = new PrinterSettings();
                    myDoc.PrinterSettings.PrinterName = settings.PrinterName;
                    myDoc.Print();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show("Error " + err.ToString(), "Error");
            }
        }
        private void printPagenumber(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;

            Font regular = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Regular);
            Font bold = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Bold);
            Font smaller = new Font(FontFamily.GenericSansSerif, 8.0f, FontStyle.Regular);
            Font bigger = new Font(FontFamily.GenericSansSerif, 11.0f, FontStyle.Regular);
            Font bigbold = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Bold);


            String Temp = "";

            if (Settings.Default.MYSQLPassword == null)
            {

            }
            else
            {
                Temp = Settings.Default.MYSQLPassword;
            }

            var conn_info = "Server=" + Settings.Default.MYSQLServer + ";Port=" + 3306 + ";Database=" + Settings.Default.MYSQLDatabase + ";Uid=" + Settings.Default.MYSQLUsername + ";Pwd=" + Temp + ";SslMode=none";
            bool isConn = false;
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(conn_info);
                conn.Open();




                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select * from checkout where order_number=(SELECT distinct(order_number) from checkout where print='0' limit 1) ";


                MySqlDataReader reader = cmd.ExecuteReader();

                String nofaktur = "";
                int currentgap = 40;
                int gap = 15;
                int desclength = " Description                                    ".Length;
                int numlength = "NO  ".Length;
                int amtlength = "   A".Length;

                int keterangan = "NO   D                                       A".Length;

                int extra = 0;
                int count = 0;
                String tempstringagain = "";
                currentgap = currentgap + gap;
                currentgap = currentgap + gap;
                while (reader.Read())
                {
                    nofaktur = reader.GetString("order_number");

                    String cutnumber = nofaktur.Substring(reader.GetString("order_number").Length-3,3);

                    graphics.DrawString(cutnumber, bigbold, Brushes.Black, 120, currentgap);

                    count++;
                }
                currentgap = currentgap + gap;
                currentgap = currentgap + gap;
                currentgap = currentgap + gap;

                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);
                graphics.DrawLine(Pens.Black, 20, currentgap + 5, 260, currentgap + 5);
                reader.Close();

                MySqlCommand cmdiappenjualan = conn.CreateCommand();
                cmdiappenjualan.CommandText = "SELECT * from iappenjualan where no_faktur='" + nofaktur + "'";
                MySqlDataReader iappenjualan = cmdiappenjualan.ExecuteReader();

                Double totalall = 0.0d;

                while (iappenjualan.Read())
                {

                    graphics.DrawString("Queue Number", bigbold, Brushes.Black, 30, 0);
                    graphics.DrawString("Date : " + iappenjualan.GetDateTime("TANGGAL").ToString("dd/MM/yyyy  hh:mm tt"), smaller, Brushes.Black, 30, 25);
                    graphics.DrawLine(Pens.Black, 20, 40, 260, 40);
                    graphics.DrawLine(Pens.Black, 20, 45, 260, 45);

                }
                iappenjualan.Close();




            }
            catch (Exception exp)
            {
                MessageBox.Show("ERROR : " + exp.ToString(), "Error");
            }




            //for (int i = 0; i < itemList.Count; i++)
            //{
            //    graphics.DrawString(itemList[i].ToString(), regular, Brushes.Black, 20, 150 + i * 20);
            //}

            //print footer
            //...

            regular.Dispose();
            bold.Dispose();

            // Check to see if more pages are to be printed.

        }
        private void printPageorder(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;

            Font regular = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Regular);
            Font bold = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Bold);
            Font smaller = new Font(FontFamily.GenericSansSerif, 8.0f, FontStyle.Regular);
            Font bigger = new Font(FontFamily.GenericSansSerif, 11.0f, FontStyle.Regular);
            Font bigbold = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Bold);

            String Temp = "";

            if (Settings.Default.MYSQLPassword == null)
            {
                
            }
            else
            {
                Temp = Settings.Default.MYSQLPassword;
            }

            var conn_info = "Server=" + Settings.Default.MYSQLServer + ";Port=" + 3306 + ";Database=" + Settings.Default.MYSQLDatabase + ";Uid=" + Settings.Default.MYSQLUsername + ";Pwd=" + Temp + ";SslMode=none";
            bool isConn = false;
            MySqlConnection conn = null;
            try
            {

                conn = new MySqlConnection(conn_info);
                conn.Open();




                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select * from checkout where order_number=(SELECT distinct(order_number) from checkout where print='0' limit 1) ";


                MySqlDataReader reader = cmd.ExecuteReader();

                String nofaktur = "";
                int currentgap = 75;
                int gap = 15;
                int desclength = " Description                                    ".Length;
                int numlength = "NO  ".Length;
                int amtlength = "   A".Length;

                int keterangan = "NO   D                                       A".Length;

                int extra = 0;
                int count = 0;
                String tempstringagain = "";

                while (reader.Read())
                {
                    nofaktur = reader.GetString("order_number");
                    count++;
                    String tempno = "";
                    String tempdesc = "";
                    String keterdesc = "";
                    String tempamt = "";

                    tempno = count + ".";
                    while (tempno.Length < numlength)
                    {
                        tempno = tempno + " ";
                    }

                    tempdesc = "   " + reader.GetString("nama_stock") ;
                    if (tempdesc.Length > desclength)
                    {
                        extra = 1;
                        tempstringagain = tempdesc;
                        tempdesc = tempdesc.Substring(0, desclength);
                        tempstringagain = tempstringagain.Substring(desclength, tempstringagain.Length-desclength);
                    }
                    else
                    {
                        while (tempdesc.Length < desclength)
                        {
                            tempdesc = tempdesc + " ";
                        }
                    }

                    //String.Format("{0:n0}", 1234); //Output: 1,234.00

                    //string.Format("{0:n0}", 9876);

                    tempamt = String.Format("{0:n0}", reader.GetDouble("qty"));
                    while (tempamt.Length < amtlength)
                    {
                        tempamt = " " + tempamt;
                    }
                    currentgap = currentgap + gap;
                    graphics.DrawString(tempno + tempdesc, smaller, Brushes.Black, 10, currentgap);
                    graphics.DrawString(tempamt, smaller, Brushes.Black, 230, currentgap);

                    if (extra == 1)
                    {
                        currentgap = currentgap + 10;
                        graphics.DrawString(tempstringagain, smaller, Brushes.Black, 10 + numlength, currentgap);
                        extra = 0;
                    }

                    int extra2 = 0;

                    currentgap = currentgap + 15;

                    String extraketerangan = "";

                    keterdesc = reader.GetString("note");

                    if (keterangan < keterdesc.Length)
                    {
                        extra2 = 1;
                        extraketerangan = keterdesc.Substring(keterangan, keterdesc.Length-keterangan);
                        keterdesc = keterdesc.Substring(0, keterangan);
                    }

                    graphics.DrawString(keterdesc, smaller, Brushes.Black, 10 + numlength, currentgap);

                    if (extra2 == 1) {
                        currentgap = currentgap + 10;
                        graphics.DrawString(extraketerangan, smaller, Brushes.Black, 10 + numlength, currentgap);
                        extra2 = 0;
                    }

                    currentgap = currentgap + gap;

                    
                    //graphics.DrawString("COD | DESCRICAO                      | QTY | X | Vir Unit | Vir Total |", bold, Brushes.Black, 10, 221);
                }

                currentgap = currentgap + gap;

                graphics.DrawLine(Pens.Black, 20, currentgap, 260, currentgap);
                graphics.DrawLine(Pens.Black, 20, currentgap + 5, 260, currentgap + 5);
                reader.Close();

                MySqlCommand cmdiappenjualan = conn.CreateCommand();
                cmdiappenjualan.CommandText = "SELECT * from iappenjualan where no_faktur='" + nofaktur + "'";
                MySqlDataReader iappenjualan = cmdiappenjualan.ExecuteReader();

                Double totalall = 0.0d;

                while (iappenjualan.Read())
                {

                    graphics.DrawString("*NEW ORDER*", bigbold, Brushes.Black, 30, 0);
                    graphics.DrawString("R.No : " + iappenjualan.GetString("NO_FAKTUR"), smaller, Brushes.Black, 30, 20);
                    graphics.DrawString(" By : " + iappenjualan.GetString("USER_ID"), smaller, Brushes.Black, 10, 35);
                    graphics.DrawString("Date : " + iappenjualan.GetDateTime("TANGGAL").ToString("dd/MM/yyyy  hh:mm tt"), smaller, Brushes.Black, 30, 50);
                    graphics.DrawLine(Pens.Black, 20, 65, 260, 65);
                    graphics.DrawLine(Pens.Black, 20, 70, 260, 70);

                }
                iappenjualan.Close();



            }
            catch (Exception exp)
            {
                MessageBox.Show("ERROR : " + exp.ToString(), "Error");
            }


            regular.Dispose();
            bold.Dispose();
            
        }
        public void executeclick() {
            if (connectionstatus == 0)
            {

                if (Settings.Default.MYSQLDatabase != null || Settings.Default.MYSQLDatabase != "" && Settings.Default.MYSQLServer != null || Settings.Default.MYSQLServer != "" && Settings.Default.MYSQLUsername != null || Settings.Default.MYSQLUsername != "")
                {

                    String Temp = "";

                    if (Settings.Default.MYSQLPassword == null)
                    {

                    }
                    else
                    {
                        Temp = Settings.Default.MYSQLPassword;
                    }

                    if (mysqlform.checkconn(Settings.Default.MYSQLServer, 3306, Settings.Default.MYSQLDatabase, Settings.Default.MYSQLUsername, Temp))
                    {
                        MessageBox.Show("Connected", "Info");
                        connectionstatus = 1;
                        label1.Text = "Monitoring";
                        label2.Text = "Connected";
                        button5.Text = "Connected";

                        aTimer = new System.Windows.Forms.Timer();
                        aTimer.Tick += timer_Tick;
                        aTimer.Start();
                        aTimer.Interval = Timercount;
                        //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                        //aTimer.Interval = 5000;
                        //aTimer.Enabled = true;

                    }
                    else {
                        MessageBox.Show("Error Occured", "Info");
                        label2.Text = "Disconnected";
                        button5.Text = "Disconnected";
                        label1.Text = "Not Monitoring";
                    }
                }
                else {
                    mysqlform forms = new mysqlform();
                    forms.ShowDialog();
                }
            }
            else {
                aTimer.Stop();
                connectionstatus = 0;
                label2.Text = "Disconnected";
                label1.Text = "Not Monitoring";
                button5.Text = "Disconnected";
            }
        }
    }
}
