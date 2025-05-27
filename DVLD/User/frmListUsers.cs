using DVLD.Classes;
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
    public partial class frmListUsers : Form
    {


        private static DataTable _dtAllUsers ;


        public frmListUsers()
        {
            InitializeComponent();
        }

        // Declare a delegate
        public delegate void DataBackEventHandler(object sender, string NewPassword);

        // Declare an event using the delegate
        public event DataBackEventHandler DataBack;
        enum enFillterBy : byte
        {
            None = 0,
            UserID,
            UserName,
            PersonID,
            FullName,
            IsAcive

        }

        enum enIUserActivationStatus : byte
        {
            All = 0,
            Yes,
            No
        }

        enFillterBy _enFillter = enFillterBy.None;
        enIUserActivationStatus _enIUserActivation = enIUserActivationStatus.All;
        private void _ReteriveNumberOfRows()
        {
            lblRecordsCount.Text = dgvUsers.Rows.Count.ToString();
        }
        private int _RetertiveUserID()
        {
            return (int)dgvUsers.CurrentRow.Cells[0].Value;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void frmListUsers_Load(object sender, EventArgs e)
        {
            _dtAllUsers = await clsUser.GetAllUsersAsync();
            dgvUsers.DataSource = _dtAllUsers;
            cbFilterBy.SelectedIndex = 0;
            _ReteriveNumberOfRows();

            if (dgvUsers.RowCount > 0)
            {
                dgvUsers.Columns[0].HeaderText = "User ID";
                dgvUsers.Columns[0].Width = 110;

                dgvUsers.Columns[1].HeaderText = "Person ID";
                dgvUsers.Columns[1].Width = 120;

                dgvUsers.Columns[2].HeaderText = "Full Name";
                dgvUsers.Columns[2].Width = 350;

                dgvUsers.Columns[3].HeaderText = "UserName";
                dgvUsers.Columns[3].Width = 120;

                dgvUsers.Columns[4].HeaderText = "Is Active";
                dgvUsers.Columns[4].Width = 120;
            }
           
        }

        private void cbFilterBy_SelectedIndexChanged(object sender, EventArgs e)
        {

            if ((enFillterBy)cbFilterBy.SelectedIndex == enFillterBy.IsAcive)
            {
                txtFilterValue.Visible= false;
                cbIsActive.Visible = true;
                cbIsActive.Focus();
                cbIsActive.SelectedIndex = 0;
            } 
            
            else

            {
                
                txtFilterValue.Visible = ((enFillterBy)cbFilterBy.SelectedIndex != enFillterBy.None) ;
                cbIsActive.Visible = false;

                txtFilterValue.Text = "";
                txtFilterValue.Focus();
            }

           
        }

        private void txtFilterValue_TextChanged(object sender, EventArgs e)
        {
            string FilterColumn = "";
            //Map Selected Filter to real Column name 
            switch ((enFillterBy)cbFilterBy.SelectedIndex)
            {
                case enFillterBy.UserID:
                    FilterColumn = "UserID";
                    break;
                case enFillterBy.UserName:
                    FilterColumn = "UserName";
                    break;

                case enFillterBy.PersonID:
                    FilterColumn = "PersonID";
                    break;

        
                case enFillterBy.FullName:
                    FilterColumn = "FullName";
                    break;

                default:
                    FilterColumn = "None";
                    break;

            }

            //Reset the filters in case nothing selected or filter value conains nothing.
            if (txtFilterValue.Text.Trim() == "" || FilterColumn == "None")
            {
                _dtAllUsers.DefaultView.RowFilter = "";
                _ReteriveNumberOfRows();
                return;
            }


            if (FilterColumn != "FullName" && FilterColumn != "UserName")
                //in this case we deal with numbers not string.
                _dtAllUsers.DefaultView.RowFilter = string.Format("[{0}] = {1}", FilterColumn, txtFilterValue.Text.Trim());
            else
                _dtAllUsers.DefaultView.RowFilter = string.Format("[{0}] LIKE '{1}%'", FilterColumn, txtFilterValue.Text.Trim());

            _ReteriveNumberOfRows();
        }

        private void cbIsActive_SelectedIndexChanged(object sender, EventArgs e)
        {

             
          string FilterColumn = "IsActive";
          //string FilterValue =cbIsActive.Text;
            enIUserActivationStatus FilterValue = (enIUserActivationStatus)cbIsActive.SelectedIndex;
            string Value = string.Empty;
            switch (FilterValue)
            {
                case enIUserActivationStatus.All:
                    break;
                case enIUserActivationStatus.Yes:
                    Value = "1";
                    break;
                case enIUserActivationStatus.No:
                    Value = "0"; 
                    break;
            }


            if (FilterValue == enIUserActivationStatus.All)
                _dtAllUsers.DefaultView.RowFilter = "";  
            else
                //in this case we deal with numbers not string.
                _dtAllUsers.DefaultView.RowFilter = string.Format("[{0}] = {1}", FilterColumn, Value);

            _ReteriveNumberOfRows();


        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            frmAddUpdateUser Frm1 = new frmAddUpdateUser ();
            Frm1.ShowDialog();
            frmListUsers_Load(null, null);  
        }

        private void _UpdatePassword(object sender, string NewPassword)
        {
            DataBack?.Invoke(this, NewPassword);
        }
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

            frmAddUpdateUser Frm1 = new frmAddUpdateUser((int)_RetertiveUserID());
            Frm1.DataBack += _UpdatePassword;
            Frm1.ShowDialog();
            frmListUsers_Load(null, null);

        }

        private void addtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmAddUpdateUser Frm1 = new frmAddUpdateUser();
            Frm1.ShowDialog();
            frmListUsers_Load(null, null);

        }

        private void dgvUsers_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            frmUserInfo Frm1 = new frmUserInfo((int)_RetertiveUserID());

            Frm1.ShowDialog();
           
        }

        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmUserInfo Frm1 = new frmUserInfo((int)_RetertiveUserID());
            Frm1.ShowDialog();
           
        }

        private void ChangePasswordtoolStripMenuItem_Click(object sender, EventArgs e)
        {

            int UserID = (int)_RetertiveUserID();
            frmChangePassword Frm1 = new frmChangePassword(UserID);
            Frm1.DataBack += _UpdatePassword;
            Frm1.ShowDialog();

        }

        private void txtFilterValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            //we allow number incase person id or user id is selected.
            /*
                 e.Handled = true; → يتم منع الإدخال ❌
                 e.Handled = false; → يُسمح بالإدخال ✅
             */
            if ((enFillterBy)cbFilterBy.SelectedIndex == enFillterBy.PersonID|| (enFillterBy)cbFilterBy.SelectedIndex == enFillterBy.UserID)
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private async void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int UserID = (int)_RetertiveUserID();

            if (clsGlobal.CurrentUser.UserID == UserID)
            {
                MessageBox.Show("You cannot delete the currently logged-in user. Please log in with a different user account to delete this user.",
                                "Deletion Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (await clsUser.DeleteUserAsync(UserID))
            {

                MessageBox.Show("User has been deleted successfully", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                frmListUsers_Load(null, null);
            }

            else
                MessageBox.Show("User is not delted due to data connected to it.", "Faild", MessageBoxButtons.OK, MessageBoxIcon.Error);


            


        }
    }
}
