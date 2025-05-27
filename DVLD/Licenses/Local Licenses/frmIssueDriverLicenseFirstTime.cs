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

namespace DVLD.DriverLicense
{
    public partial class frmIssueDriverLicenseFirstTime : Form
    {
        private int _LocalDrivingLicenseApplicationID;
        private  clsLocalDrivingLicenseApplication _LocalDrivingLicenseApplication;

        public frmIssueDriverLicenseFirstTime(int LocalDrivingLicenseApplicationID)
        {
            InitializeComponent();
            _LocalDrivingLicenseApplicationID = LocalDrivingLicenseApplicationID;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private async void frmIssueDriverLicenseFirstTime_Load(object sender, EventArgs e)
        {

            txtNotes.Focus();            
            _LocalDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(_LocalDrivingLicenseApplicationID);

            if (_LocalDrivingLicenseApplication ==null)
            {

                MessageBox.Show("No Applicaiton with ID=" + _LocalDrivingLicenseApplicationID.ToString(), "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }


            if (! await _LocalDrivingLicenseApplication.PassedAllTestsAsync())
            {

                MessageBox.Show("Person Should Pass All Tests First.", "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            int LicenseID = await _LocalDrivingLicenseApplication.GetActiveLicenseIDAsync();
            if (LicenseID !=-1)
            {
                 
                MessageBox.Show("Person already has License before with License ID=" + LicenseID.ToString() , "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;

            }

            await ctrlDrivingLicenseApplicationInfo1.LoadApplicationInfoByLocalDrivingAppID(_LocalDrivingLicenseApplicationID);
          


        }

        private async void btnIssueLicense_Click(object sender, EventArgs e)
        {
            int LicenseID=  await _LocalDrivingLicenseApplication.IssueLicenseForTheFirtsTimeAsync(txtNotes.Text.Trim(),clsGlobal.CurrentUser.UserID);

            if (LicenseID != -1)
            {
                MessageBox.Show("License Issued Successfully with License ID = " + LicenseID.ToString(),
                    "Succeeded",MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
              else
            {
                MessageBox.Show("License Was not Issued ! " ,
                 "Faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
