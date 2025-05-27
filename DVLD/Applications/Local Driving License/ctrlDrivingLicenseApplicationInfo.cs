using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DVLD_Buisness;
using DVLD.Classes;
using static System.Net.Mime.MediaTypeNames;
using DVLD.Tests;
using DVLD.DriverLicense;

namespace DVLD.Controls.ApplicationControls
{
    public partial class ctrlDrivingLicenseApplicationInfo: UserControl
    {

        private clsLocalDrivingLicenseApplication _LocalDrivingLicenseApplication;

        private int _LocalDrivingLicenseApplicationID = -1;

        private int _LicenseID = -1;

        public int LocalDrivingLicenseApplicationID
        {
            get { return _LocalDrivingLicenseApplicationID; }
        }


        public ctrlDrivingLicenseApplicationInfo()
        {
            InitializeComponent();
        }

        public async Task LoadApplicationInfoByLocalDrivingAppID(int LocalDrivingLicenseApplicationID)
        {
            _LocalDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(LocalDrivingLicenseApplicationID);
            if (_LocalDrivingLicenseApplication == null)
            {
                _ResetLocalDrivingLicenseApplicationInfo();
                

                MessageBox.Show("No Application with ApplicationID = " + LocalDrivingLicenseApplicationID.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
              
                await _FillLocalDrivingLicenseApplicationInfo();
        }

        public async Task LoadApplicationInfoByApplicationID(int ApplicationID)
        {
            _LocalDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByApplicationIDAsync(ApplicationID);
            if (_LocalDrivingLicenseApplication == null)
            {
                _ResetLocalDrivingLicenseApplicationInfo();


                MessageBox.Show("No Application with ApplicationID = " + LocalDrivingLicenseApplicationID.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
         
                await _FillLocalDrivingLicenseApplicationInfo();
        }

        private async Task _FillLocalDrivingLicenseApplicationInfo()
        {
            _LicenseID = await _LocalDrivingLicenseApplication.GetActiveLicenseIDAsync();
           
            //incase there is license enable the show link.
            llShowLicenceInfo.Enabled = (_LicenseID != -1);

           
            lblLocalDrivingLicenseApplicationID.Text = _LocalDrivingLicenseApplication.LocalDrivingLicenseApplicationID.ToString();
            lblAppliedFor.Text = await clsLicenseClass.GetClassNameByIDAsync( _LocalDrivingLicenseApplication.LicenseClassID);
            lblPassedTests.Text = (await _LocalDrivingLicenseApplication.GetPassedTestCountAsync()).ToString() +"/3" ; 
            await ctrlApplicationBasicInfo1.LoadApplicationInfo(_LocalDrivingLicenseApplication.ApplicationID);

        }

        private void _ResetLocalDrivingLicenseApplicationInfo()
        {
            _LocalDrivingLicenseApplicationID = -1;
            ctrlApplicationBasicInfo1.ResetApplicationInfo();
            lblLocalDrivingLicenseApplicationID.Text = "[????]";
            lblAppliedFor.Text = "[????]";


        }

        private async void llShowLicenceInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmShowLicenseInfo frm = new frmShowLicenseInfo(await _LocalDrivingLicenseApplication.GetActiveLicenseIDAsync());
            frm.ShowDialog();

        }
    }
}
