using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFA_Sample_A;

namespace LoginPage
{
    public partial class LoginPage : Form
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void UseridTxt_Enter(object sender, EventArgs e)
        {
            if (UseridTxt.Text == "Enter Your UserID")
            {
                UseridTxt.Text = "";
                UseridTxt.ForeColor = Color.Black;
            }
        }

        private void UseridTxt_Leave(object sender, EventArgs e)
        {
            if (UseridTxt.Text == "")
            {
                UseridTxt.Text = "Enter Your UserID";
                UseridTxt.ForeColor = Color.LightGray;
            }
        }

        private void PasswordTxt_Enter(object sender, EventArgs e)
        {
            if (PasswordTxt.Text == "Enter Your Password")
            {
                PasswordTxt.Text = "";
                PasswordTxt.ForeColor = Color.Black;
                PasswordTxt.UseSystemPasswordChar = true;
            }
        }

        private void PasswordTxt_Leave(object sender, EventArgs e)
        {
            if (PasswordTxt.Text == "")
            {
                PasswordTxt.Text = "Enter Your Password";
                PasswordTxt.ForeColor = Color.LightGray;
                PasswordTxt.UseSystemPasswordChar = false;
            }
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            if (UseridTxt.Text == "Enter Your UserID" || PasswordTxt.Text == "Enter Your Password")
            {
                MessageBox.Show("Please enter both UserID and Password.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string userId = this.UseridTxt.Text.Trim();
            string password = this.PasswordTxt.Text.Trim();

            string sql = $"SELECT * FROM Users WHERE UserID = '{userId}' AND Password = '{password}';";

            DataAccess da = new DataAccess();
            DataSet ds = da.ExecuteQuery(sql);

            if (ds.Tables[0].Rows.Count == 1)
            {
                string role = ds.Tables[0].Rows[0]["Role"].ToString();

                if (role == "CEO")
                {
                    CEO_Dashboard cd = new CEO_Dashboard(); 
                    this.Hide();
                    cd.Show();
                }
                else if (role == "Manager")
                {
                    Manager_Dashboard md = new Manager_Dashboard(userId);  
                    this.Hide();
                    md.Show();
                }
                else if (role == "Auditor")
                {
                    Auditor_Dashboard ad = new Auditor_Dashboard(userId);
                    this.Hide();
                    ad.Show();
                }
                else
                {
                    MessageBox.Show("Unknown Role.");
                }
            }
            else
            {
                MessageBox.Show("Login Invalid!!!");
            }
        }
    }
}

