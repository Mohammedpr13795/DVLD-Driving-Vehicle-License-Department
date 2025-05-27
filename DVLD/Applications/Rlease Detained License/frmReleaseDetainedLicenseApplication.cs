using DVLD.Classes;
using DVLD.DriverLicense;
using DVLD.Licenses.Controls;
using DVLD.Licenses.International_License;
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
using static DVLD.Licenses.Controls.ctrlDriverLicenseInfoWithFilter;
using static System.Net.Mime.MediaTypeNames;

namespace DVLD.Applications.Rlease_Detained_License
{
    public partial class frmReleaseDetainedLicenseApplication : Form
    {


        private int _SelectedLicenseID = -1;

        public frmReleaseDetainedLicenseApplication()
        {
            InitializeComponent();
        }

        public frmReleaseDetainedLicenseApplication(int LicenseID)
        {
            InitializeComponent();
            _SelectedLicenseID = LicenseID;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void ctrlDriverLicenseInfoWithFilter1_OnLicenseSelected(object sender , ctrlDriverLicenseInfoWithFilterEventArgs e)
        {
            _SelectedLicenseID = e.LicenseID;

            lblLicenseID.Text = _SelectedLicenseID.ToString();

            llShowLicenseHistory.Enabled = (_SelectedLicenseID != -1);

            if (_SelectedLicenseID == -1)

            {
                return;
            }


            clsLicense License = await clsLicense.FindAsync(_SelectedLicenseID);


            //if (!clsLicense.GetActiveLicenseIDByPersonID(License.DriverInfo.PersonID , License.LicenseClass))
            //{
            //    _LDLAppID = clsLDLAppBusinessLayer.RetrieveLDLAppIDByLicenseID(_LicenseID);


            //    usrDriverLicenseInfo1.LoadDriverLicenseInfoByLDLAppID(_LDLAppID, _LicenseID);
            //    MessageBox.Show($"The license with ID {_LicenseID} is not active, Choise an active license.",
            //                    "Not Allowed",
            //                    MessageBoxButtons.OK,
            //                    MessageBoxIcon.Error);

            //    if (DataBack != null) DataBack?.Invoke(_LicenseID, _LDLAppID, true, false);

            //    return;
            //}


            //ToDo: make sure the license is not detained already.
            if (!await ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.IsDetainedAsync())
            {
                MessageBox.Show("Selected License is not detained, choose another one.", "Not allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblApplicationFees.Text = ((await clsApplicationType.FindAsync((int)clsApplication.enApplicationType.ReleaseDetainedDrivingLicsense))).Fees.ToString();
            lblCreatedByUser.Text = clsGlobal.CurrentUser.UserName;

            lblDetainID.Text = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DetainedInfo.DetainID.ToString();
            lblLicenseID.Text = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.LicenseID.ToString();

            lblCreatedByUser.Text = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DetainedInfo.CreatedByUserInfo.UserName;
            lblDetainDate.Text = clsFormat.DateToShort(ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DetainedInfo.DetainDate);
            lblFineFees.Text = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DetainedInfo.FineFees.ToString();
            lblTotalFees.Text = (Convert.ToSingle(lblApplicationFees.Text) + Convert.ToSingle(lblFineFees.Text)).ToString();

            btnRelease.Enabled = true;
        }

        private void frmReleaseDetainedLicenseApplication_Activated(object sender, EventArgs e)
        {
            ctrlDriverLicenseInfoWithFilter1.txtLicenseIDFocus();
        }

        private void llShowLicenseHistory_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmShowPersonLicenseHistory frm =
             new frmShowPersonLicenseHistory(ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DriverInfo.PersonID);
            frm.ShowDialog();
        }

        private void llShowLicenseInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmShowLicenseInfo frm =
           new frmShowLicenseInfo(_SelectedLicenseID);
            frm.ShowDialog();
        }

        private async void btnRelease_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to release this detained  license?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }



            //Using Tupple
            (bool IsReleased, int ApplicationID) = await ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.ReleaseDetainedLicenseAsync(clsGlobal.CurrentUser.UserID); ;

            lblApplicationID.Text = ApplicationID.ToString();

            if (!IsReleased)
            {
                MessageBox.Show("Faild to to release the Detain License", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Detained License released Successfully ", "Detained License Released", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnRelease.Enabled = false;
            ctrlDriverLicenseInfoWithFilter1.FilterEnabled = false;
            llShowLicenseInfo.Enabled = true;
        }

        private async void frmReleaseDetainedLicenseApplication_Load(object sender, EventArgs e)
        {
            if (_SelectedLicenseID != -1)
            {
                await ctrlDriverLicenseInfoWithFilter1.LoadLicenseInfo(_SelectedLicenseID);
                ctrlDriverLicenseInfoWithFilter1.FilterEnabled = false;
            }
        }
    }
}
