using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoPrint.Properties;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace AutoPrint
{
    public partial class mysqlform : Form
    {
        MySqlConnection conn;

        string myConnectionString;
        public mysqlform()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Equals(""))
            {
                MessageBox.Show("Server must not Be Empty", "error");
            }
            else if (textBox2.Text.Equals(""))
            {
                MessageBox.Show("Database must not Be Empty", "error");
            }
            else if (textBox3.Text.Equals(""))
            {
                MessageBox.Show("Username must not Be Empty", "error");
            }
            else {

                Settings.Default.MYSQLServer  = textBox1.Text;
                Settings.Default.MYSQLDatabase = textBox2.Text;
                Settings.Default.MYSQLUsername = textBox3.Text;
                Settings.Default.MYSQLPassword = textBox4.Text;

                Settings.Default.Save();

                if (checkconn(textBox1.Text.ToString(),3306, textBox2.Text.ToString(), textBox3.Text.ToString(), textBox4.Text.ToString()))
                {
                    MessageBox.Show("Connected", "Info");
                }
                else {
                    MessageBox.Show("Error Occured", "Info");
                }
            }
        }
        public static bool checkconn(String Server , int Port , String Database , String Username , String Password)
        {
            var conn_info = "Server="+Server+ ";Port=" + Port + ";Database=" +Database + ";Uid=" + Username + ";Pwd=" + Password + ";SslMode=none";
            bool isConn = false;
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(conn_info);
                conn.Open();
                isConn = true;
            }
            catch (ArgumentException a_ex)
            {
                MessageBox.Show(a_ex.ToString(), "error argument");
                /*
                Console.WriteLine("Check the Connection String.");
                Console.WriteLine(a_ex.Message);
                Console.WriteLine(a_ex.ToString());
                */
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString(), "error exception");
                /*string sqlErrorMessage = "Message: " + ex.Message + "\n" +
                "Source: " + ex.Source + "\n" +
                "Number: " + ex.Number;
                Console.WriteLine(sqlErrorMessage);
                */
                isConn = false;
                switch (ex.Number)
                {
                    //http://dev.mysql.com/doc/refman/5.0/en/error-messages-server.html
                    case 1042: // Unable to connect to any of the specified MySQL hosts (Check Server,Port)
                        break;
                    case 0: // Access denied (Check DB name,username,password)
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return isConn;
        }

        private void mysqlform_Load(object sender, EventArgs e)
        {
            textBox1.Text = Settings.Default.MYSQLServer;
            textBox2.Text = Settings.Default.MYSQLDatabase;
            textBox3.Text = Settings.Default.MYSQLUsername;
            textBox4.Text = Settings.Default.MYSQLPassword;
        }
    }
}
