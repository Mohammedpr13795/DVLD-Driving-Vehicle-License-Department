using DVLD.Classes;
using DVLD.Controls;
using DVLD.People;
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
using static System.Net.Mime.MediaTypeNames;

namespace DVLD.Applications
{


    public partial class frmAddUpdateLocalDrivingLicesnseApplication: Form
    {

        public enum enMode { AddNew = 0, Update = 1 };

        private enMode _Mode;
        private int _LocalDrivingLicenseApplicationID = -1;
        private int _SelectedPersonID = -1;
        clsLocalDrivingLicenseApplication _LocalDrivingLicenseApplication;

        public frmAddUpdateLocalDrivingLicesnseApplication()
        {
            InitializeComponent();
            _Mode = enMode.AddNew;
        }

        public frmAddUpdateLocalDrivingLicesnseApplication(int LocalDrivingLicenseApplicationID)
        {
            InitializeComponent();

            _Mode = enMode.Update;
            _LocalDrivingLicenseApplicationID = LocalDrivingLicenseApplicationID;

        }

        private async Task _FillLicenseClassesInComoboBox()
        {
            DataTable dtLicenseClasses = await clsLicenseClass.GetAllLicenseClassesAsync();

            foreach (DataRow row in dtLicenseClasses.Rows)
            {
                cbLicenseClass.Items.Add(row["ClassName"]);
            }
        }

        private async Task _ResetDefualtValues()
        {
            //this will initialize the reset the defaule values
            await _FillLicenseClassesInComoboBox();

            lblTitle.Text = this.Text = (_Mode == enMode.AddNew ? "Add New" : "Update") + " Local Driving License Application";

            if (_Mode == enMode.AddNew)
            {
                
                _LocalDrivingLicenseApplication = new clsLocalDrivingLicenseApplication();
                ctrlPersonCardWithFilter1.FilterFocus();
                tpApplicationInfo.Enabled = false;
              
                cbLicenseClass.SelectedIndex = 2;
                lblFees.Text = (await clsApplicationType.FindAsync((int)clsApplication.enApplicationType.NewDrivingLicense)).Fees.ToString();  
                lblApplicationDate.Text= DateTime.Now.ToShortDateString();
                lblCreatedByUser.Text = clsGlobal.CurrentUser.UserName;
            }
            else
            {



                tpApplicationInfo.Enabled = true;
                btnSave.Enabled = true;
             

            }

        }

        private async void _LoadData()
        {

            ctrlPersonCardWithFilter1.FilterEnabled = false;
            _LocalDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(_LocalDrivingLicenseApplicationID);

            if (_LocalDrivingLicenseApplication == null)
            {
                MessageBox.Show("No Application with ID = " + _LocalDrivingLicenseApplicationID, "Application Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();

                return;
            }

            ctrlPersonCardWithFilter1.LoadPersonInfo(_LocalDrivingLicenseApplication.ApplicantPersonID);
            lblLocalDrivingLicebseApplicationID.Text = _LocalDrivingLicenseApplication.LocalDrivingLicenseApplicationID.ToString();
            lblApplicationDate.Text = clsFormat.DateToShort( _LocalDrivingLicenseApplication.ApplicationDate);
            cbLicenseClass.SelectedIndex = cbLicenseClass.FindString((await clsLicenseClass.FindAsync(_LocalDrivingLicenseApplication.LicenseClassID)).ClassName);
            lblFees.Text= _LocalDrivingLicenseApplication.PaidFees.ToString();
            lblCreatedByUser.Text = (await clsUser.FindByUserIDAsync(_LocalDrivingLicenseApplication.CreatedByUserID)).UserName;

        }

        private void DataBackEvent(object sender, int PersonID)
        {
            // Handle the data received
            _SelectedPersonID=PersonID;
            ctrlPersonCardWithFilter1.LoadPersonInfo(PersonID);
           

        }

        private async void frmAddUpdateLocalDrivingLicesnseApplication_Load(object sender, EventArgs e)
        {
            await _ResetDefualtValues();

            if (_Mode==enMode.Update)
            {
                _LoadData();
            }
           
        }

        private void btnApplicationInfoNext_Click(object sender, EventArgs e)
        {

          

            if (_Mode == enMode.Update)
            {
                btnSave.Enabled = true;
                tpApplicationInfo.Enabled = true;
                tcApplicationInfo.SelectedTab = tcApplicationInfo.TabPages["tpApplicationInfo"];
                return;
            }


            //incase of add new mode.
            if (ctrlPersonCardWithFilter1.PersonID != -1)
            {
        
                    btnSave.Enabled = true;
                    tpApplicationInfo.Enabled = true;
                    tcApplicationInfo.SelectedTab = tcApplicationInfo.TabPages["tpApplicationInfo"];
   
            }

            else

            {
                MessageBox.Show("Please Select a Person", "Select a Person", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ctrlPersonCardWithFilter1.FilterFocus();
            }
        }

        private async Task<bool> _IsEligibleForLicenseByAge()
        {
            int minimumAllowedAge = await clsLicenseClass.GetMinimumAllowedAgeByIDAsync(cbLicenseClass.SelectedIndex + 1);
            DateTime birthDate = ctrlPersonCardWithFilter1.SelectedPersonInfo.DateOfBirth;

            int personAge = DateTime.Now.Year - birthDate.Year;

            if (DateTime.Now < birthDate.AddYears(personAge))
            {
                personAge--;
            }

            return personAge >= minimumAllowedAge;
        }
        private async void btnSave_Click(object sender, EventArgs e)
        {

            
            int LicenseClassID = (await clsLicenseClass.FindAsync(cbLicenseClass.Text)).LicenseClassID;


            int ActiveApplicationID = await clsApplication.GetActiveApplicationIDForLicenseClassAsync(_SelectedPersonID, clsApplication.enApplicationType.NewDrivingLicense, LicenseClassID);



            if (ActiveApplicationID != -1)
            {
                MessageBox.Show("Choose another License Class, the selected Person Already have an active application for the selected class with id=" + ActiveApplicationID, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cbLicenseClass.Focus();
                return;
            }


            //check if user already have issued license of the same driving  class.
            if ( await clsLicense.IsLicenseExistByPersonID(ctrlPersonCardWithFilter1.PersonID, LicenseClassID))
            {

                MessageBox.Show("Person already have a license with the same applied driving class, Choose diffrent driving class", "Not allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Check if the user his/her age is under the age of the license class
            if (!await _IsEligibleForLicenseByAge())
            {
                MessageBox.Show("The person is not eligible to apply for the license due to age restrictions.", "Eligibility Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            _LocalDrivingLicenseApplication.ApplicantPersonID = ctrlPersonCardWithFilter1.PersonID; 
            _LocalDrivingLicenseApplication.ApplicationDate = DateTime.Now;
            _LocalDrivingLicenseApplication.ApplicationTypeID = 1;
            _LocalDrivingLicenseApplication.ApplicationStatus = clsApplication.enApplicationStatus.New;
            _LocalDrivingLicenseApplication.LastStatusDate = DateTime.Now;
            _LocalDrivingLicenseApplication.PaidFees = Convert.ToSingle(lblFees.Text);
            _LocalDrivingLicenseApplication.CreatedByUserID = clsGlobal.CurrentUser.UserID;
            _LocalDrivingLicenseApplication.LicenseClassID= LicenseClassID;


            if (await _LocalDrivingLicenseApplication.SaveAsync())
            {
                lblLocalDrivingLicebseApplicationID.Text = _LocalDrivingLicenseApplication.LocalDrivingLicenseApplicationID.ToString();
                //change form mode to update.
                _Mode = enMode.Update;
                lblTitle.Text = "Update Local Driving License Application";

                MessageBox.Show("Data Saved Successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
                MessageBox.Show("Error: Data Is not Saved Successfully.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);



        }

        private void ctrlPersonCardWithFilter1_OnPersonSelected(object sender , ctrlPersonCardWithFilter.ctrlPersonCardWithFilterEventArgs e)
        {
            _SelectedPersonID = e.PersonID;

        }

        private void frmAddUpdateLocalDrivingLicesnseApplication_Activated(object sender, EventArgs e)
        {
            ctrlPersonCardWithFilter1.FilterFocus();
        }
    }
}
