using DVLD.Classes;
using DVLD.Global_Classes;
using DVLD_Buisness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.Login
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }
        byte _NumberOfTry = 3;
        bool _IsRemember = false;


        private async Task<bool> IsTriesEnded(byte NumberOfTries)
        {
            if (NumberOfTries == 0)
            {
                MessageBox.Show("Attempts have ended. Please try again after one hour.", "", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                await clsEventLogger.LogEvent("This Error accured due to 3 failed login attempts at: \"{DateTime.Now}\"." , EventLogEntryType.Error);
                return true;
            }
            return false;
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void _UpdatePassword(object sender, string NewPassword)
        {
            txtPassword.Text = NewPassword;
        }
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (await IsTriesEnded(_NumberOfTry))
                return;

            clsUser user = await clsUser.FindByUsernameAndPasswordAsync(txtUserName.Text.Trim(),txtPassword.Text.Trim());

            if (user != null) 
            { 

                if (chkRememberMe.Checked )
                {
                    //store username and password
                    //clsGlobal.RememberUsernameAndPassword(txtUserName.Text.Trim(), txtPassword.Text.Trim());
                    await clsGlobal.SaveCredentialsToRegistry(txtUserName.Text.Trim(), txtPassword.Text.Trim());
                    _IsRemember = true;

                } 
                  else
                {
                    //store empty username and password
                    await clsGlobal.SaveCredentialsToRegistry("", "");

                }

                //incase the user is not active
                if (!user.IsActive )
                {

                    txtUserName.Focus();
                    MessageBox.Show("Your accound is not Active, Contact Admin.", "In Active Account", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                 clsGlobal.CurrentUser = user;
                clsGlobal.CurrentUser.IsRemember = _IsRemember;
                 this.Hide();
                //frmMain frm = new frmMain(this);
                frmMain frm = new frmMain();
                frm.DataBack += _UpdatePassword;
                if(!chkRememberMe.Checked) { txtUserName.Text = string.Empty; txtPassword.Text = string.Empty;}
                frm.ShowDialog();
                this.Show();


            }
            else
            {
                _NumberOfTry--;
                txtUserName.Focus();
                MessageBox.Show($"Invalid username or password. You have {_NumberOfTry} attempts remaining.",
                "Wrong Credintials",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
                //MessageBox.Show("Invalid Username/Password.", "Wrong Credintials", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }    

        }



        private async void frmLogin_Load(object sender, EventArgs e)
        {

            clsGlobal.clsCredentialResult credentialResult = await clsGlobal.GetCredentialsFromRegistry();

            if (credentialResult.Success && !string.IsNullOrEmpty(credentialResult.Username) && !string.IsNullOrEmpty(credentialResult.Password))
            {
                txtUserName.Text = credentialResult.Username;
                txtPassword.Text = credentialResult.Password;
                chkRememberMe.Checked = true;
            }
            else
                chkRememberMe.Checked = false;

        }
    }
}
