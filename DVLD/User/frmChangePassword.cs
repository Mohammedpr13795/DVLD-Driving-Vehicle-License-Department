using DVLD.Classes;
using DVLD.Login;
using DVLD_Buisness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.User
{
    public partial class frmChangePassword : Form
    {
        private int _UserID;
        private clsUser _User;

        // Declare a delegate
        public delegate void DataBackEventHandler(object sender, string NewPassword);

        // Declare an event using the delegate
        public event DataBackEventHandler DataBack;

        public frmChangePassword(int UserID )
        {
            InitializeComponent();

            _UserID=UserID;
        }

        private void _ResetDefualtValues()
        {
            txtCurrentPassword.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
            txtCurrentPassword.Focus(); 
        }

        private async void frmChangePassword_Load(object sender, EventArgs e)
        {
             _ResetDefualtValues();

              _User = await clsUser.FindByUserIDAsync(_UserID);

            if (_User == null)
            {
                //Here we dont continue becuase the form is not valid
                MessageBox.Show("Could not Find User with id = " + _UserID,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 this.Close();

                return;

            }
            await ctrlUserCard1.LoadUserInfo(_UserID);
            txtCurrentPassword.Focus();
            if (clsGlobal.CurrentUser.IsRemember) _User.IsRemember = true;

        }
        private bool _CheckIsNullOrEmpty(TextBox txt, CancelEventArgs e , string MessageError)
        {
            if (string.IsNullOrEmpty(txt.Text))
            {
                e.Cancel = true;
                txt.Focus();
                erproValidting.SetError(txt, MessageError);
                return true;
            }
            return false;
        }
        private bool _VerifyCurrentPassword(TextBox txt, CancelEventArgs e)
        {
            string hasedPassword = clsCryptoHelper.ComputeHashPassword(txtCurrentPassword.Text.Trim());
            if (_User.Password != hasedPassword)
            {
                e.Cancel = true;
                txt.Focus();
                erproValidting.SetError(txt, " The entered password does not match the current password.");
                return true;
            }
            ;
            return false;
        }
        private void _ClearValidationError(TextBox txt, CancelEventArgs e)
        {
            //e.Cancel = false;
            erproValidting.SetError(txt, null);
        }

        private void txtCurrentPassword_Validating(object sender, CancelEventArgs e)
        {
            if (_CheckIsNullOrEmpty(txtCurrentPassword, e , "Current password cannot be blank")) return;

            else if (_VerifyCurrentPassword(txtCurrentPassword, e)) return;

            else _ClearValidationError(txtCurrentPassword, e);

            //if (string.IsNullOrEmpty(txtCurrentPassword.Text.Trim()))
            //{
            //    e.Cancel = true;
            //    erproValidting.SetError(txtCurrentPassword, "Current password cannot be blank");
            //    return;
            //}
            //else
            //{
            //    erproValidting.SetError(txtCurrentPassword, null);
            //};

            //if (_User.Password != txtCurrentPassword.Text.Trim())
            //{
            //    e.Cancel = true;
            //    erproValidting.SetError(txtCurrentPassword, "Current password is wrong!");
            //    return;
            //}
            //else
            //{
            //    erproValidting.SetError(txtCurrentPassword, null);
            //};
        }

        private void txtNewPassword_Validating(object sender, CancelEventArgs e)
        {
            if (_CheckIsNullOrEmpty(txtNewPassword, e , "New Password cannot be blank")) return;

            else _ClearValidationError(txtNewPassword, e);

            //if (string.IsNullOrEmpty(txtNewPassword.Text.Trim()))
            //{
            //    e.Cancel = true;
            //    erproValidting.SetError(txtNewPassword, "New Password cannot be blank");
            //}
            //else
            //{
            //    erproValidting.SetError(txtNewPassword, null);
            //};
        }

        private bool _IsPasswordConfirm(TextBox txt, CancelEventArgs e)
        {
            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                e.Cancel = true;
                txt.Focus();
                erproValidting.SetError(txt, "Password Confirmation does not match New Password!");
                return true;
            }
            return false;
        }
        private void txtConfirmPassword_Validating(object sender, CancelEventArgs e)
        {
            if (_CheckIsNullOrEmpty(txtConfirmPassword, e , "Confirm Password cannot be blank")) return;

            else if (_IsPasswordConfirm(txtConfirmPassword, e)) return;

            else _ClearValidationError(txtConfirmPassword, e);


            //if (txtConfirmPassword.Text.Trim() != txtNewPassword.Text.Trim())
            //{
            //    e.Cancel = true;
            //    erproValidting.SetError(txtConfirmPassword, "Password Confirmation does not match New Password!");
            //}
            //else
            //{
            //    erproValidting.SetError(txtConfirmPassword, null);
            //};
        }
        private async Task _UpdatePassword()
        {
            if (_User.IsRemember)
            {
                await clsGlobal.SaveCredentialsToRegistry(_User.UserName.Trim(), txtNewPassword.Text.Trim());
                //frmLogin.UserNameAndPassword[0] = frmMain.User.UserName;
                //frmLogin.UserNameAndPassword[1] = frmMain.User.Password;
                //File.WriteAllLines(frmLogin.FilePath, frmLogin.UserNameAndPassword);
            }

        }
        private async void btnSave_Click(object sender, EventArgs e)
        {


            
            if (!this.ValidateChildren())
            {
                //Here we dont continue becuase the form is not valid
                MessageBox.Show("Some fileds are not valide!, put the mouse over the red icon(s) to see the erro",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (txtCurrentPassword.Text == txtNewPassword.Text)
            {
                MessageBox.Show("The new password cannot be the same as the current password. Please enter a new password.", "Invalid Password",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _User.Password = txtNewPassword.Text;

            if (await _User.SaveAsync())
            {
                MessageBox.Show("Password Changed Successfully.",
                   "Saved.", MessageBoxButtons.OK, MessageBoxIcon.Information );
                if (clsGlobal.CurrentUser.UserID == _UserID)
                {
                    await clsGlobal.UpdatePasswordAsync(_User, txtConfirmPassword.Text); _ResetDefualtValues();
                    clsGlobal.clsCredentialResult credentialResult = await clsGlobal.GetCredentialsFromRegistry();
                    DataBack?.Invoke(this, credentialResult.Password);
                    return;
                }
                //DataBack?.Invoke(this, _User.Password);
            }
            else
            {
                MessageBox.Show("An Erro Occured, Password did not change.",
                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
