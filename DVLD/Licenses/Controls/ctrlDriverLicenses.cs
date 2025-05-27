using DVLD.Controls;
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
using DVLD;
using DVLD.Licenses.International_Licenses;

namespace DVLD.Licenses.Local_Licenses.Controls
{
    public partial class ctrlDriverLicenses : UserControl
    {
        private int _DriverID;
        private clsDriver _Driver ;
        private DataTable _dtDriverLocalLicensesHistory;
        private DataTable _dtDriverInternationalLicensesHistory;

        public ctrlDriverLicenses()
        {
            InitializeComponent();
        }

        private void _ReteriveNumberOfRows(char LicneseType)
        {
            if (LicneseType == 'L')
                lblLocalLicensesRecords.Text = dgvLocalLicensesHistory.Rows.Count.ToString();

            else
                lblInternationalLicensesRecords.Text = dgvInternationalLicensesHistory.Rows.Count.ToString();


        }
        private async Task _LoadLocalLicenseInfo()
        {

            _dtDriverLocalLicensesHistory = await clsDriver.GetLicensesAsync(_DriverID);


            dgvLocalLicensesHistory.DataSource = _dtDriverLocalLicensesHistory;
            _ReteriveNumberOfRows('L');
            //lblLocalLicensesRecords.Text = dgvLocalLicensesHistory.Rows.Count.ToString();

            if (dgvLocalLicensesHistory.Rows.Count > 0)
            {
                dgvLocalLicensesHistory.Columns[0].HeaderText = "Lic.ID";
                dgvLocalLicensesHistory.Columns[0].Width = 110;

                dgvLocalLicensesHistory.Columns[1].HeaderText = "App.ID";
                dgvLocalLicensesHistory.Columns[1].Width = 110;

                dgvLocalLicensesHistory.Columns[2].HeaderText = "Class Name";
                dgvLocalLicensesHistory.Columns[2].Width = 270;

                dgvLocalLicensesHistory.Columns[3].HeaderText = "Issue Date";
                dgvLocalLicensesHistory.Columns[3].Width = 170;

                dgvLocalLicensesHistory.Columns[4].HeaderText = "Expiration Date";
                dgvLocalLicensesHistory.Columns[4].Width = 170;

                dgvLocalLicensesHistory.Columns[5].HeaderText = "Is Active";
                dgvLocalLicensesHistory.Columns[5].Width = 110;

            }
            else
                dgvLocalLicensesHistory.Enabled = false;
        }

        private async Task _LoadInternationalLicenseInfo()
        {

            _dtDriverInternationalLicensesHistory = await clsDriver.GetInternationalLicenses(_DriverID);


            dgvInternationalLicensesHistory.DataSource = _dtDriverInternationalLicensesHistory;
            _ReteriveNumberOfRows('I');
            //lblInternationalLicensesRecords.Text = dgvInternationalLicensesHistory.Rows.Count.ToString();

            if (dgvInternationalLicensesHistory.Rows.Count > 0)
            {
                dgvInternationalLicensesHistory.Columns[0].HeaderText = "Int.License ID";
                dgvInternationalLicensesHistory.Columns[0].Width = 160;

                dgvInternationalLicensesHistory.Columns[1].HeaderText = "Application ID";
                dgvInternationalLicensesHistory.Columns[1].Width = 130;

                dgvInternationalLicensesHistory.Columns[2].HeaderText = "L.License ID";
                dgvInternationalLicensesHistory.Columns[2].Width = 130;

                dgvInternationalLicensesHistory.Columns[3].HeaderText = "Issue Date";
                dgvInternationalLicensesHistory.Columns[3].Width = 180;

                dgvInternationalLicensesHistory.Columns[4].HeaderText = "Expiration Date";
                dgvInternationalLicensesHistory.Columns[4].Width = 180;

                dgvInternationalLicensesHistory.Columns[5].HeaderText = "Is Active";
                dgvInternationalLicensesHistory.Columns[5].Width = 120;

            }
            else
                dgvInternationalLicensesHistory.Enabled = false;
        }

        public async Task LoadInfo(int DriverID)
        {
            _DriverID = DriverID;
            _Driver = await clsDriver.FindByDriverID(_DriverID);

            if(_Driver == null)
            {
                MessageBox.Show("There is no driver with id = " + _DriverID , "Not Alwod",MessageBoxButtons.OK , MessageBoxIcon.Error); 
                return;
            }
            await _LoadLocalLicenseInfo();
            await _LoadInternationalLicenseInfo();

        }

        public async Task  LoadInfoByPersonID(int PersonID)
        {
            
            _Driver = await clsDriver.FindByPersonID(PersonID);
            

            if (_Driver == null)
            {
                MessageBox.Show("There is no person driver linked with person id = " + PersonID, "Not Alwod", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

           _DriverID = _Driver.DriverID;

            await _LoadLocalLicenseInfo();
            await _LoadInternationalLicenseInfo();
        }

        private void showLicenseInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LicenseID = (int)dgvLocalLicensesHistory.CurrentRow.Cells[0].Value;
            DriverLicense.frmShowLicenseInfo frm = new DriverLicense.frmShowLicenseInfo(LicenseID);
            frm.ShowDialog();
            
        }

        public void Clear()
        {
            _dtDriverLocalLicensesHistory.Clear();
            _dtDriverInternationalLicensesHistory.Clear();

        }

        private void InternationalLicenseHistorytoolStripMenuItem_Click(object sender, EventArgs e)
        {
            int InternationalLicenseID = (int)dgvInternationalLicensesHistory.CurrentRow.Cells[0].Value;
            frmShowInternationalLicenseInfo frm = new frmShowInternationalLicenseInfo(InternationalLicenseID);
            frm.ShowDialog();
        }

    }
}
