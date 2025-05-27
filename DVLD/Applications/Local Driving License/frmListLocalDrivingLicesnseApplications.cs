using DVLD.Applications;
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
using DVLD.DriverLicense;
using System.Security.Cryptography;
using DVLD.Drivers;
using DVLD.Licenses.International_License;

namespace DVLD.Tests
{
    public partial class frmListLocalDrivingLicesnseApplications : Form
    {
        private DataTable _dtAllLocalDrivingLicenseApplications;
        enum enFillterBy : byte
        {
            None = 0,
            LDLAppID,
            NationalNo,
            FullName,
            Status

        }

        enum enApplicationsStatus : byte
        {
            New = 0,
            Cancelled,
            Compeleted
        }

        enFillterBy enFillter = enFillterBy.None;
        enApplicationsStatus eApplicationsStatus = enApplicationsStatus.New;
        public frmListLocalDrivingLicesnseApplications()
        {
            InitializeComponent();
        }
        private void _ReteriveNumberOfRows()
        {
            lblRecordsCount.Text = dgvLocalDrivingLicenseApplications.Rows.Count.ToString();
        }
        private int _ReteriveLDLApplID()
        {
            if (dgvLocalDrivingLicenseApplications.Rows.Count > 0)
            {
                return (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;
            }
            return -1;

        }
        private async void frmListLocalDrivingLicesnseApplications_Load(object sender, EventArgs e)
        {
            _dtAllLocalDrivingLicenseApplications = await clsLocalDrivingLicenseApplication.GetAllLocalDrivingLicenseApplicationsAsync();
            dgvLocalDrivingLicenseApplications.DataSource = _dtAllLocalDrivingLicenseApplications;

            _ReteriveNumberOfRows();
            if (dgvLocalDrivingLicenseApplications.Rows.Count>0)
            {

                dgvLocalDrivingLicenseApplications.Columns[0].HeaderText = "L.D.L.AppID";
                dgvLocalDrivingLicenseApplications.Columns[0].Width = 120;

                dgvLocalDrivingLicenseApplications.Columns[1].HeaderText = "Driving Class";
                dgvLocalDrivingLicenseApplications.Columns[1].Width = 300;

                dgvLocalDrivingLicenseApplications.Columns[2].HeaderText = "National No.";
                dgvLocalDrivingLicenseApplications.Columns[2].Width = 150;

                dgvLocalDrivingLicenseApplications.Columns[3].HeaderText = "Full Name";
                dgvLocalDrivingLicenseApplications.Columns[3].Width = 350;

                dgvLocalDrivingLicenseApplications.Columns[4].HeaderText = "Application Date";
                dgvLocalDrivingLicenseApplications.Columns[4].Width = 170;

                dgvLocalDrivingLicenseApplications.Columns[5].HeaderText = "Passed Tests";
                dgvLocalDrivingLicenseApplications.Columns[5].Width = 150;
            }

            cbFilterBy.SelectedIndex = 0;


        }

        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmLocalDrivingLicenseApplicationInfo frm =
                        new frmLocalDrivingLicenseApplicationInfo(_ReteriveLDLApplID());
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);

        }

        private void cbFilterBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtFilterValue.Visible = ((enFillterBy)cbFilterBy.SelectedIndex != enFillterBy.None);

            if (txtFilterValue.Visible)
            {
                txtFilterValue.Text = "";
                txtFilterValue.Focus();
            }

            if ((enFillterBy)cbFilterBy.SelectedIndex == enFillterBy.Status)
            {
                txtFilterValue.Visible = false;
                CBApplicationsStatues.Visible = true;
                CBApplicationsStatues.Focus();
            }

            else

            {

                txtFilterValue.Visible = ((enFillterBy)cbFilterBy.SelectedIndex != enFillterBy.None);
                CBApplicationsStatues.Visible = false;

                txtFilterValue.Text = "";
                txtFilterValue.Focus();
            }



            _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = "";
            _ReteriveNumberOfRows();
        }

        private void txtFilterValue_TextChanged(object sender, EventArgs e)
        {

            string FilterColumn = "";
            //Map Selected Filter to real Column name 
            switch ((enFillterBy)cbFilterBy.SelectedIndex)
            {

                case enFillterBy.LDLAppID:
                    FilterColumn = "LocalDrivingLicenseApplicationID";
                    break;

                case enFillterBy.NationalNo:
                    FilterColumn = "NationalNo";
                    break;


                case enFillterBy.FullName:
                    FilterColumn = "FullName";
                    break;

                case enFillterBy.Status:
                    FilterColumn = "Status";
                    break;


                default:
                    FilterColumn = "None";
                    break;

            }

            //Reset the filters in case nothing selected or filter value conains nothing.
            if (txtFilterValue.Text.Trim() == "" || FilterColumn == "None")
            {
                _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = "";
                _ReteriveNumberOfRows();
                return;
            }


            if (FilterColumn == "LocalDrivingLicenseApplicationID")
                //in this case we deal with integer not string.
                _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = string.Format("[{0}] = {1}", FilterColumn, txtFilterValue.Text.Trim());
            else
                _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = string.Format("[{0}] LIKE '{1}%'", FilterColumn, txtFilterValue.Text.Trim());

            _ReteriveNumberOfRows();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();

            frmAddUpdateLocalDrivingLicesnseApplication frm =
                         new frmAddUpdateLocalDrivingLicesnseApplication(LocalDrivingLicenseApplicationID);
            frm.ShowDialog();

            frmListLocalDrivingLicesnseApplications_Load(null, null);
        }

        private void txtFilterValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            //we allow number incase L.D.L.AppID id is selected.
            if (cbFilterBy.Text == "L.D.L.AppID")
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private async void vistionTestToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();

            clsTestAppointment Appointment = await clsTestAppointment.GetLastTestAppointmentAsync(LocalDrivingLicenseApplicationID, clsTestType.enTestType.VisionTest);

            if (Appointment == null)
            {
                MessageBox.Show("No Vision Test Appointment Found!", "Set Appointment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            frmTakeTest frm = new frmTakeTest(Appointment.TestAppointmentID, clsTestType.enTestType.VisionTest);
            frm.ShowDialog();


        }

        private async void writtenTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();


            if (!await clsLocalDrivingLicenseApplication.DoesPassTestTypeAsync(LocalDrivingLicenseApplicationID, clsTestType.enTestType.VisionTest))
            {
                MessageBox.Show("Person Should Pass the Vision Test First!", "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            clsTestAppointment Appointment = await clsTestAppointment.GetLastTestAppointmentAsync(LocalDrivingLicenseApplicationID, clsTestType.enTestType.WrittenTest);


            if (Appointment == null)
            {
                MessageBox.Show("No Written Test Appointment Found!", "Set Appointment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            frmTakeTest frm = new frmTakeTest(Appointment.TestAppointmentID, clsTestType.enTestType.WrittenTest);
            frm.ShowDialog();

        }

        private async void streetTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();

            if (!await clsLocalDrivingLicenseApplication.DoesPassTestTypeAsync(LocalDrivingLicenseApplicationID, clsTestType.enTestType.WrittenTest))
            {
                MessageBox.Show("Person Should Pass the Written Test First!", "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            clsTestAppointment Appointment = await clsTestAppointment.GetLastTestAppointmentAsync(LocalDrivingLicenseApplicationID, clsTestType.enTestType.StreetTest);


            if (Appointment == null)
            {
                MessageBox.Show("No Street Test Appointment Found!", "Set Appointment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            frmTakeTest frm = new frmTakeTest(Appointment.TestAppointmentID, clsTestType.enTestType.StreetTest);
            frm.ShowDialog();


        }

        private void _ScheduleTest(clsTestType.enTestType TestType)
        {

            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();
            frmListTestAppointments frm = new frmListTestAppointments(LocalDrivingLicenseApplicationID, TestType);
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);

        }
      
        private void scheduleVisionTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ScheduleTest(clsTestType.enTestType.VisionTest);
        }

        private void scheduleWrittenTestToolStripMenuItem_Click(object sender, EventArgs e)
        {

            _ScheduleTest(clsTestType.enTestType.WrittenTest);
        }

        private void scheduleStreetTestToolStripMenuItem_Click(object sender, EventArgs e)
        {

            _ScheduleTest(clsTestType.enTestType.StreetTest);
        }

        private void btnAddNewApplication_Click(object sender, EventArgs e)
        {
            frmAddUpdateLocalDrivingLicesnseApplication frm = new frmAddUpdateLocalDrivingLicesnseApplication();
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);
        }

        private void issueDrivingLicenseFirstTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();
            frmIssueDriverLicenseFirstTime frm = new frmIssueDriverLicenseFirstTime(LocalDrivingLicenseApplicationID);
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);
        }

        private async void cmsApplications_Opening(object sender, CancelEventArgs e)
        {
            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();
            clsLocalDrivingLicenseApplication LocalDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(LocalDrivingLicenseApplicationID);

            int TotalPassedTests = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[5].Value;

            bool LicenseExists = await LocalDrivingLicenseApplication.IsLicenseIssuedAsync();

            //Enabled only if person passed all tests and Does not have license. 
            issueDrivingLicenseFirstTimeToolStripMenuItem.Enabled = (TotalPassedTests == 3) && !LicenseExists;
            
            showLicenseToolStripMenuItem.Enabled = LicenseExists;
            editToolStripMenuItem.Enabled = !LicenseExists && (LocalDrivingLicenseApplication.ApplicationStatus == clsApplication.enApplicationStatus.New);
            ScheduleTestsMenue.Enabled = !LicenseExists;

            //Enable/Disable Cancel Menue Item
            //We only canel the applications with status=new.
            CancelApplicaitonToolStripMenuItem.Enabled = (LocalDrivingLicenseApplication.ApplicationStatus==clsApplication.enApplicationStatus.New);

            //Enable/Disable Delete Menue Item
            //We only allow delete incase the application status is new not complete or Cancelled.
            DeleteApplicationToolStripMenuItem.Enabled = 
                (LocalDrivingLicenseApplication.ApplicationStatus == clsApplication.enApplicationStatus.New );



            //Enable Disable Schedule menue and it's sub menue
            bool PassedVisionTest = await LocalDrivingLicenseApplication.DoesPassTestTypeAsync(clsTestType.enTestType.VisionTest); ;
            bool PassedWrittenTest = await LocalDrivingLicenseApplication.DoesPassTestTypeAsync(clsTestType.enTestType.WrittenTest);
            bool PassedStreetTest = await LocalDrivingLicenseApplication.DoesPassTestTypeAsync(clsTestType.enTestType.StreetTest);

            ScheduleTestsMenue.Enabled = (!PassedVisionTest || !PassedWrittenTest || !PassedStreetTest) && (LocalDrivingLicenseApplication.ApplicationStatus == clsApplication.enApplicationStatus.New);

            if (ScheduleTestsMenue.Enabled)
            {
                //To Allow Schdule vision test, Person must not passed the same test before.
                scheduleVisionTestToolStripMenuItem.Enabled = !PassedVisionTest;

                //To Allow Schdule written test, Person must pass the vision test and must not passed the same test before.
                scheduleWrittenTestToolStripMenuItem.Enabled = PassedVisionTest && !PassedWrittenTest;

                //To Allow Schdule steet test, Person must pass the vision * written tests, and must not passed the same test before.
                scheduleStreetTestToolStripMenuItem.Enabled = PassedVisionTest && PassedWrittenTest && !PassedStreetTest;

            }



        }

        private async void showLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();

            clsLocalDrivingLicenseApplication localApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(LocalDrivingLicenseApplicationID);
            int LicenseID = await localApplication.GetActiveLicenseIDAsync();
            
            
            if (LicenseID != -1)
            {
                frmShowLicenseInfo frm = new frmShowLicenseInfo(LicenseID);
                frm.ShowDialog();

            }
            else
            {
                MessageBox.Show("No License Found!", "No License", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


        }

        private async void CancelApplicaitonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure do want to cancel this application?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();

            clsLocalDrivingLicenseApplication LocalDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(LocalDrivingLicenseApplicationID);

            if (LocalDrivingLicenseApplication != null)
            {
                if (await LocalDrivingLicenseApplication.CancelAsync())
                {
                    MessageBox.Show("Application Cancelled Successfully.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //refresh the form again.
                    frmListLocalDrivingLicesnseApplications_Load(null, null);
                }
                else
                {
                    MessageBox.Show("Could not cancel applicatoin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void DeleteApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure do want to delete this application?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();

            clsLocalDrivingLicenseApplication LocalDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(LocalDrivingLicenseApplicationID);

            if (LocalDrivingLicenseApplication != null)
            {
                if (await LocalDrivingLicenseApplication.DeleteAsync())
                {
                    MessageBox.Show("Application Deleted Successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //refresh the form again.
                    frmListLocalDrivingLicesnseApplications_Load(null, null);
                }
                else
                {
                    MessageBox.Show("Could not delete applicatoin, other data depends on it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void showPersonLicenseHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = _ReteriveLDLApplID();
            clsLocalDrivingLicenseApplication localDrivingLicenseApplication = await clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseIDAsync(LocalDrivingLicenseApplicationID);

            frmShowPersonLicenseHistory frm = new frmShowPersonLicenseHistory(localDrivingLicenseApplication.ApplicantPersonID);
            frm.ShowDialog();
        }

        private void CBApplicationsStatues_SelectedIndexChanged(object sender, EventArgs e)
        {
            string FilterColumn = "Status";
            //string FilterValue =cbIsActive.Text;
            enApplicationsStatus FilterValue = (enApplicationsStatus)CBApplicationsStatues.SelectedIndex;
            string Value = string.Empty;
            switch (FilterValue)
            {
                case enApplicationsStatus.New:
                    Value = "New";
                    break;
                case enApplicationsStatus.Cancelled:
                    Value = "Cancelled";
                    break;
                case enApplicationsStatus.Compeleted:
                    Value = "Completed";
                    break;
            }


            //if (FilterValue == enIUserActivationStatus.All)
            //    _dtAllUsers.DefaultView.RowFilter = "";

            //in this case we deal with numbers not string.
            _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = string.Format("[{0}] LIKE '{1}%'", FilterColumn, Value);

        }
    }
}
