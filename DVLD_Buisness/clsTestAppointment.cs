using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

// Assuming clsTestType, clsApplication exist and are accessible
// using DVLD_Buisness;

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Test Appointment entity and handles related business logic.
    /// Find and GetLastTestAppointment methods are AASYNCHRONOUSLYLY.
    /// Save, GetAll, and other Get methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsTestAppointment
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties
        public int TestAppointmentID { get; set; }
        public clsTestType.enTestType TestTypeID { get; set; } // Enum for logical representation
        public int LocalDrivingLicenseApplicationID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public float PaidFees { get; set; }
        public int CreatedByUserID { get; set; }
        public bool IsLocked { get; set; }
        public int RetakeTestApplicationID { get; set; }

        // Related object (Lazy loaded or loaded in constructor)
        public clsApplication RetakeTestAppInfo { get; private set; } // Loaded AASYNCHRONOUSLYLYly in constructor

        public Task<int > TestID   
        {
            get { return  _GetTestIDAsync(); }   
          
        }
        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsTestAppointment()
        {
            this.TestAppointmentID = -1;
            this.TestTypeID = clsTestType.enTestType.VisionTest; // Default type
            this.LocalDrivingLicenseApplicationID = -1; // Must be set before saving
            this.AppointmentDate = DateTime.MinValue; // Or DateTime.Now?
            this.PaidFees = 0;
            this.CreatedByUserID = -1;
            this.IsLocked = false; // Default to not locked
            this.RetakeTestApplicationID = -1; // Default to no retake app
            this.RetakeTestAppInfo = null;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find methods.
        /// </summary>
        private clsTestAppointment(int testAppointmentID, clsTestType.enTestType testTypeID,
           int localDrivingLicenseApplicationID, DateTime appointmentDate, float paidFees,
           int createdByUserID, bool isLocked, int retakeTestApplicationID , clsApplication RetakeTestAppInfo)
        {
            this.TestAppointmentID = testAppointmentID;
            this.TestTypeID = testTypeID;
            this.LocalDrivingLicenseApplicationID = localDrivingLicenseApplicationID;
            this.AppointmentDate = appointmentDate;
            this.PaidFees = paidFees;
            this.CreatedByUserID = createdByUserID;
            this.IsLocked = isLocked;
            this.RetakeTestApplicationID = retakeTestApplicationID;

            // Load RetakeTestAppInfo AASYNCHRONOUSLYLYly if ID is valid
            if (this.RetakeTestApplicationID != -1)
            {
                this.RetakeTestAppInfo = RetakeTestAppInfo;
            }
            else
            {
                this.RetakeTestAppInfo = null;
            }

            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new test appointment data via DAL.
        /// </summary>
        private async Task<bool> _AddNewTestAppointmentAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.TestAppointmentID = await clsTestAppointmentData.AddNewTestAppointmentAsync(
                (int)this.TestTypeID, this.LocalDrivingLicenseApplicationID, this.AppointmentDate,
                this.PaidFees, this.CreatedByUserID, this.RetakeTestApplicationID);
            return (this.TestAppointmentID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the test appointment data via DAL.
        /// </summary>
        private async Task<bool> _UpdateTestAppointmentAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsTestAppointmentData.UpdateTestAppointmentAsync(
                this.TestAppointmentID, (int)this.TestTypeID, this.LocalDrivingLicenseApplicationID,
                this.AppointmentDate, this.PaidFees, this.CreatedByUserID, this.IsLocked, this.RetakeTestApplicationID);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a test appointment by its ID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="TestAppointmentIDParam">The ID to search for.</param>
        /// <returns>A clsTestAppointment object if found; otherwise, null.</returns>
        public static async Task<clsTestAppointment> FindAsync(int TestAppointmentIDParam) // Sync signature
        {
            // Declare variables for ref parameters
            int TestTypeIDInt = -1, LocalDrivingLicenseApplicationID = -1, CreatedByUserID = -1, RetakeTestApplicationID = -1;
            DateTime AppointmentDate = DateTime.MinValue;
            float PaidFees = 0;
            bool IsFound= false , IsLocked = false;

            (IsFound, TestTypeIDInt, LocalDrivingLicenseApplicationID, AppointmentDate,
             PaidFees, CreatedByUserID, IsLocked, RetakeTestApplicationID) = await clsTestAppointmentData.GetTestAppointmentInfoByIDAsync(TestAppointmentIDParam);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                clsApplication RetakeTestAppInfo = await clsApplication.FindBaseApplicationAsync(RetakeTestApplicationID); // Assumes sync Find

                // Use private constructor
                return new clsTestAppointment(TestAppointmentIDParam, (clsTestType.enTestType)TestTypeIDInt, LocalDrivingLicenseApplicationID,
                                              AppointmentDate, PaidFees, CreatedByUserID, IsLocked, RetakeTestApplicationID , RetakeTestAppInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY gets the last test appointment for a specific application and test type.
        /// </summary>
        /// <returns>A clsTestAppointment object if found; otherwise, null.</returns>
        public static async Task<clsTestAppointment> GetLastTestAppointmentAsync(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestTypeID) // Sync signature
        {
            // Declare variables for ref parameters
            int TestAppointmentID = -1, CreatedByUserID = -1, RetakeTestApplicationID = -1;
            DateTime AppointmentDate = DateTime.MinValue;
            float PaidFees = 0;
            bool IsFound = false , IsLocked = false;

            (IsFound, TestAppointmentID, AppointmentDate, PaidFees, CreatedByUserID, IsLocked, RetakeTestApplicationID)
                = await clsTestAppointmentData.GetLastTestAppointmentAsync(LocalDrivingLicenseApplicationID, (int)TestTypeID);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                clsApplication RetakeTestAppInfo = await clsApplication.FindBaseApplicationAsync(RetakeTestApplicationID); 
                // Use private constructor
                return new clsTestAppointment(TestAppointmentID, TestTypeID, LocalDrivingLicenseApplicationID,
                                             AppointmentDate, PaidFees, CreatedByUserID, IsLocked, RetakeTestApplicationID , RetakeTestAppInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all test appointments.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        public static async Task<DataTable> GetAllTestAppointmentsAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsTestAppointmentData.GetAllTestAppointmentsAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable of appointments for the current application instance and a specific test type.
        /// </summary>
        /// <param name="TestType">The type of test.</param>
        /// <returns>A Task returning a DataTable.</returns>
        public async Task<DataTable> GetApplicationTestAppointmentsPerTestTypeAsync(clsTestType.enTestType TestType) // Async suffix, instance method
        {
            // Call static ASYNC method
            return await GetApplicationTestAppointmentsPerTestTypeAsync(this.LocalDrivingLicenseApplicationID, TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable of appointments for a specific application ID and test type.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <param name="TestType">The type of test.</param>
        /// <returns>A Task returning a DataTable.</returns>
        public static async Task<DataTable> GetApplicationTestAppointmentsPerTestTypeAsync(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestType) // Async suffix, static method
        {
            // Call ASYNC DAL method
            return await clsTestAppointmentData.GetApplicationTestAppointmentsPerTestTypeAsync(LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current test appointment object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async suffix
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewTestAppointmentAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    return await _UpdateTestAppointmentAsync();
            }
            return false;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to get the TestID associated with this appointment.
        /// </summary>
        /// <returns>Task returning the TestID or -1.</returns>
        private async Task<int> _GetTestIDAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsTestAppointmentData.GetTestIDAsync(this.TestAppointmentID);
        }

        // --- Original AASYNCHRONOUSLYLY Methods (Consider replacing calls with Async versions) ---
        /*
        public static DataTable GetAllTestAppointments() => GetAllTestAppointmentsAsync().GetAwaiter().GetResult();
        public DataTable GetApplicationTestAppointmentsPerTestType(clsTestType.enTestType TestTypeID) => GetApplicationTestAppointmentsPerTestTypeAsync(TestTypeID).GetAwaiter().GetResult();
        public static DataTable GetApplicationTestAppointmentsPerTestType(int LocalDrivingLicenseApplicationID,clsTestType.enTestType TestTypeID) => GetApplicationTestAppointmentsPerTestTypeAsync(LocalDrivingLicenseApplicationID, TestTypeID).GetAwaiter().GetResult();
        public bool Save() => SaveAsync().GetAwaiter().GetResult();
        private int _GetTestID() => _GetTestIDAsync().GetAwaiter().GetResult(); // Property still uses blocking call
        */

    } // End Class clsTestAppointment
} // End Namespace