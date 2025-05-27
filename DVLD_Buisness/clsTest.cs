using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

// Assuming clsTestAppointment, clsTestType exist and are accessible
// using DVLD_Buisness;

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Test result entity and handles related business logic.
    /// Find methods are AASYNCHRONOUSLYLY.
    /// Save, GetAll, and GetPassedTestCount/PassedAllTests methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsTest
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties
        public int TestID { get; set; }
        public int TestAppointmentID { get; set; }
        public clsTestAppointment TestAppointmentInfo { get; private set; } // Loaded AASYNCHRONOUSLYLYly in Find/Constructor
        public bool TestResult { get; set; }
        public string Notes { get; set; }
        public int CreatedByUserID { get; set; }


        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsTest()
        {
            this.TestID = -1;
            this.TestAppointmentID = -1;
            this.TestResult = false; // Default to fail? Or require setting?
            this.Notes = null;
            this.CreatedByUserID = -1;
            this.TestAppointmentInfo = null;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find methods.
        /// </summary>
        private clsTest(int testID, int testAppointmentID, bool testResult, string notes, int createdByUserID , clsTestAppointment TestAppointmentInfo)
        {
            this.TestID = testID;
            this.TestAppointmentID = testAppointmentID;
            this.TestResult = testResult;
            this.Notes = notes;
            this.CreatedByUserID = createdByUserID;

            this.TestAppointmentInfo =  TestAppointmentInfo; // Assumes sync Find
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new test data via DAL.
        /// </summary>
        private async Task<bool> _AddNewTestAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.TestID = await clsTestData.AddNewTestAsync(this.TestAppointmentID,
                this.TestResult, this.Notes, this.CreatedByUserID);
            return (this.TestID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the test data via DAL.
        /// </summary>
        private async Task<bool> _UpdateTestAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsTestData.UpdateTestAsync(this.TestID, this.TestAppointmentID,
                this.TestResult, this.Notes, this.CreatedByUserID);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a test by its ID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="TestIDParam">The ID to search for.</param>
        /// <returns>A clsTest object if found; otherwise, null.</returns>
        public static async Task<clsTest> FindAsync(int TestIDParam) // Sync signature
        {
            // Declare variables for ref parameters
            int TestAppointmentID = -1, CreatedByUserID = -1;
            bool IsFound = false , TestResult = false;
            string Notes = null;
            (IsFound, TestAppointmentID, TestResult, Notes, CreatedByUserID) = await clsTestData.GetTestInfoByIDAsync(TestIDParam);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                clsTestAppointment TestAppointmentInfo = await clsTestAppointment.FindAsync(TestAppointmentID); // Assumes sync Find

                // Use private constructor
                return new clsTest(TestIDParam, TestAppointmentID, TestResult, Notes, CreatedByUserID , TestAppointmentInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds the last test taken by a person for a specific license class and test type.
        /// </summary>
        /// <returns>A clsTest object if found; otherwise, null.</returns>
        public static async Task<clsTest> FindLastTestPerPersonAndLicenseClassAYNC // Sync signature
            (int PersonID, int LicenseClassID, clsTestType.enTestType TestTypeID)
        {
            // Declare variables for ref parameters
            int TestID = -1, TestAppointmentID = -1, CreatedByUserID = -1;
            bool IsFound = false , TestResult = false;
            string Notes = null;
            (IsFound, TestID, TestAppointmentID, TestResult, Notes, CreatedByUserID) = await clsTestData.GetLastTestByPersonAndTestTypeAndLicenseClassAsync(PersonID, LicenseClassID, (int)TestTypeID);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                clsTestAppointment TestAppointmentInfo = await clsTestAppointment.FindAsync(TestAppointmentID); 
                // Use private constructor
                return new clsTest(TestID, TestAppointmentID, TestResult, Notes, CreatedByUserID, TestAppointmentInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all tests.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        public static async Task<DataTable> GetAllTestsAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsTestData.GetAllTestsAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current test object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async suffix
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewTestAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    return await _UpdateTestAsync();
            }
            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the count of passed tests for a specific application ID.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <returns>Task returning the count of passed tests (byte).</returns>
        public static async Task<byte> GetPassedTestCountAsync(int LocalDrivingLicenseApplicationID) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsTestData.GetPassedTestCountAsync(LocalDrivingLicenseApplicationID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if all required tests (assumed to be 3) have been passed for an application.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <returns>Task returning true if passed count is 3, false otherwise.</returns>
        public static async Task<bool> PassedAllTestsAsync(int LocalDrivingLicenseApplicationID) // Async suffix
        {
            // Call the async GetPassedTestCount method and check result
            return (await GetPassedTestCountAsync(LocalDrivingLicenseApplicationID) == 3);
        }


        // --- Original AASYNCHRONOUSLYLY Methods (Consider replacing calls with Async versions) ---
        /*
        public static DataTable GetAllTests() => GetAllTestsAsync().GetAwaiter().GetResult();
        public bool Save() => SaveAsync().GetAwaiter().GetResult();
        public static byte GetPassedTestCount(int LocalDrivingLicenseApplicationID) => GetPassedTestCountAsync(LocalDrivingLicenseApplicationID).GetAwaiter().GetResult();
        public static bool PassedAllTests(int LocalDrivingLicenseApplicationID) => PassedAllTestsAsync(LocalDrivingLicenseApplicationID).GetAwaiter().GetResult();
        */

    } // End Class clsTest
} // End Namespace