using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL
// Assuming other BLL classes like clsPerson, clsLicenseClass, clsTestType, clsTest, clsDriver, clsLicense are accessible
// using DVLD_Buisness;

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Local Driving License Application, inheriting from clsApplication.
    /// Handles specific logic for local license applications and tests.
    /// Find methods are AASYNCHRONOUSLYLY. Other methods are AAASYNCHRONOUSLYLY where appropriate.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsLocalDrivingLicenseApplication : clsApplication // Inherits from clsApplication
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        // Properties specific to LocalDrivingLicenseApplication
        public int LocalDrivingLicenseApplicationID { get; set; }
        public int LicenseClassID { get; set; }

        // Consider loading LicenseClassInfo aAASYNCHRONOUSLYLYly or on demand
        public clsLicenseClass LicenseClassInfo { get; private set; } // Loaded in Find/Constructor

        public string PersonFullName
        {
            get
            {
                return base.PersonInfo.FullName;
                // return clsPerson.Find(ApplicantPersonID).FullName; 
            }

        }

        public clsLocalDrivingLicenseApplication()

        {
            this.LocalDrivingLicenseApplicationID = -1;
            this.LicenseClassID = -1;


            Mode = enMode.AddNew;

        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find methods. Loads base application data first.
        /// </summary>
        private clsLocalDrivingLicenseApplication(int LocalDrivingLicenseApplicationID, int ApplicationID, int ApplicantPersonID,
            DateTime ApplicationDate, int ApplicationTypeID,
             enApplicationStatus ApplicationStatus, DateTime LastStatusDate,
             float PaidFees, int CreatedByUserID, int LicenseClassID ,clsPerson PersonInfo ,   clsLicenseClass LicenseClassInfo)
        {

            this.LocalDrivingLicenseApplicationID = LocalDrivingLicenseApplicationID; ;
            this.ApplicationID = ApplicationID;
            this.ApplicantPersonID = ApplicantPersonID;
            this.PersonInfo = PersonInfo;
            this.ApplicationDate = ApplicationDate;
            this.ApplicationTypeID = (int)ApplicationTypeID;
            this.ApplicationStatus = ApplicationStatus;
            this.LastStatusDate = LastStatusDate;
            this.PaidFees = PaidFees;
            this.CreatedByUserID = CreatedByUserID;
            this.LicenseClassID = LicenseClassID;
            this.LicenseClassInfo = LicenseClassInfo;
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the local application specific data via DAL.
        /// Assumes the base application data has already been saved.
        /// </summary>
        private async Task<bool> _AddNewLocalDrivingLicenseApplicationAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.LocalDrivingLicenseApplicationID = await clsLocalDrivingLicenseApplicationData.AddNewLocalDrivingLicenseApplicationAsync
                (this.ApplicationID, this.LicenseClassID); // Pass base ApplicationID

            return (this.LocalDrivingLicenseApplicationID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the local application specific data via DAL.
        /// </summary>
        private async Task<bool> _UpdateLocalDrivingLicenseApplicationAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.UpdateLocalDrivingLicenseApplicationAsync
                (this.LocalDrivingLicenseApplicationID, this.ApplicationID, this.LicenseClassID);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a local driving license application by its specific ID.
        /// </summary>
        /// <returns>A clsLocalDrivingLicenseApplication object if found; otherwise, null.</returns>
        public static async Task<clsLocalDrivingLicenseApplication> FindByLocalDrivingAppLicenseIDAsync(int LocalDrivingLicenseApplicationID) // Sync signature
        {
            int ApplicationID = -1, LicenseClassID = -1;
            bool IsFound = false;
            // Call AASYNCHRONOUSLYLY DAL method to get local app info
            (IsFound , ApplicationID , LicenseClassID)= await clsLocalDrivingLicenseApplicationData.GetLocalDrivingLicenseApplicationInfoByIDAsync(LocalDrivingLicenseApplicationID);

            if (IsFound)
            {
                // Now find the base application data (AASYNCHRONOUSLYLYLY)
                clsApplication BaseApplication = await clsApplication.FindBaseApplicationAsync(ApplicationID); // Uses sync Find

                if (BaseApplication != null)
                {
                    clsLicenseClass LicenseClassInfo = await clsLicenseClass.FindAsync(LicenseClassID); // Assumes sync Find
                    clsPerson PersonInfo = await clsPerson.FindAsync(BaseApplication.ApplicantPersonID);
                    // Use the private constructor to create the derived object
                    return new clsLocalDrivingLicenseApplication(
                        LocalDrivingLicenseApplicationID, BaseApplication.ApplicationID,
                        BaseApplication.ApplicantPersonID, BaseApplication.ApplicationDate, BaseApplication.ApplicationTypeID,
                        BaseApplication.ApplicationStatus, BaseApplication.LastStatusDate,
                        BaseApplication.PaidFees, BaseApplication.CreatedByUserID, LicenseClassID , PersonInfo,LicenseClassInfo);
                }
                else
                {
                    // Base application not found, data inconsistency? Log error.
                    clsEventLogger.LogEvent($"Inconsistency: LocalDrivingLicenseApplication {LocalDrivingLicenseApplicationID} found, but base Application {ApplicationID} not found.", System.Diagnostics.EventLogEntryType.Error);
                    return null;
                }
            }
            else
                return null; // Local application not found
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a local driving license application by the base ApplicationID.
        /// </summary>
        /// <returns>A clsLocalDrivingLicenseApplication object if found; otherwise, null.</returns>
        public static async Task<clsLocalDrivingLicenseApplication> FindByApplicationIDAsync(int ApplicationID) // Sync signature
        {
            int LocalDrivingLicenseApplicationID = -1, LicenseClassID = -1;
            bool IsFound = false;
            // Call AASYNCHRONOUSLYLY DAL method to get local app info using base ApplicationID
            (IsFound , LocalDrivingLicenseApplicationID , LicenseClassID) =
                await clsLocalDrivingLicenseApplicationData.GetLocalDrivingLicenseApplicationInfoByApplicationIDAsync(ApplicationID);

            if (IsFound)
            {
                // Now find the base application data (AASYNCHRONOUSLYLYLY)
                clsApplication BaseApplication = await clsApplication.FindBaseApplicationAsync(ApplicationID); // Uses sync Find
                
                if (BaseApplication != null)
                {
                    clsLicenseClass LicenseClassInfo = await clsLicenseClass.FindAsync(LicenseClassID); // Assumes sync Find
                    clsPerson PersonInfo = await clsPerson.FindAsync(BaseApplication.ApplicantPersonID);
                    // Use the private constructor
                    return new clsLocalDrivingLicenseApplication(
                        LocalDrivingLicenseApplicationID, ApplicationID, // Use the known ApplicationID
                        BaseApplication.ApplicantPersonID, BaseApplication.ApplicationDate, BaseApplication.ApplicationTypeID,
                        BaseApplication.ApplicationStatus, BaseApplication.LastStatusDate,
                        BaseApplication.PaidFees, BaseApplication.CreatedByUserID, LicenseClassID ,PersonInfo, LicenseClassInfo);
                }
                else
                {
                    clsEventLogger.LogEvent($"Inconsistency: LocalDrivingLicenseApplication found via AppID {ApplicationID}, but base Application not found.", System.Diagnostics.EventLogEntryType.Error);
                    return null;
                }
            }
            else
                return null; // No local application found for this base ApplicationID
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current local driving license application.
        /// Saves the base application data first, then the local application data.
        /// Assumes base.SaveAsync() exists and is implemented correctly.
        /// </summary>
        /// <returns>A Task returning true if both saves were successful; false otherwise.</returns>
        public new async Task<bool> SaveAsync() // Use 'new' keyword to hide base SaveAsync if needed, or ensure base is virtual and override
        {
            // Set the base class mode before saving
            base.Mode = (clsApplication.enMode)this.Mode;
            
            // Await the base class save method first
            bool baseSaveSuccess = await base.SaveAsync(); // ASSUMES base.SaveAsync() exists

            if (!baseSaveSuccess)
            {
                return false; // Stop if base save failed
            }

            // Base save was successful, now save the local driving license part
            switch (this.Mode) // Use this.Mode or just Mode
            {
                case enMode.AddNew:
                    if (await _AddNewLocalDrivingLicenseApplicationAsync())
                    {
                        // If local add succeeds, set Mode to Update
                        this.Mode = enMode.Update; // Set derived class mode
                        base.Mode = clsApplication.enMode.Update; // Ensure base mode is also updated
                        return true;
                    }
                    else
                    {
                        // Local add failed - consider rolling back base application save? (complex)
                        // For now, just return false.
                        return false;
                    }

                case enMode.Update:
                    // Await the update method for the local part
                    return await _UpdateLocalDrivingLicenseApplicationAsync();
            }

            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all local driving license applications.
        /// </summary>
        /// <returns>A Task returning a DataTable with application info.</returns>
        public static async Task<DataTable> GetAllLocalDrivingLicenseApplicationsAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.GetAllLocalDrivingLicenseApplicationsAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY deletes the local driving license application AND the base application.
        /// Assumes base.DeleteAsync() exists and is implemented correctly.
        /// </summary>
        /// <returns>Task returning true if both deletes were successful, false otherwise.</returns>
        public new async Task<bool> DeleteAsync() // Use 'new' or override if base is virtual
        {
            bool localDeleteSuccess = false;
            bool baseDeleteSuccess = false;

            // First, delete the local driving license application record
            localDeleteSuccess = await clsLocalDrivingLicenseApplicationData.DeleteLocalDrivingLicenseApplicationAsync(this.LocalDrivingLicenseApplicationID);

            if (!localDeleteSuccess)
            {
                // Log error? Failed to delete local part.
                return false;
            }

            // If local delete succeeded, delete the base application record
            baseDeleteSuccess = await base.DeleteAsync(); // ASSUMES base.DeleteAsync() exists

            return baseDeleteSuccess; // Return the result of the base deletion
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if the latest test of a specific type was passed.
        /// </summary>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning true if passed, false otherwise.</returns>
        public async Task<bool> DoesPassTestTypeAsync(clsTestType.enTestType TestType) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.DoesPassTestTypeAsync(this.LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY checks if the prerequisites for the current test type are met.
        /// NOTE: Remains AASYNCHRONOUSLYLY because it needs immediate boolean results from potentially async checks.
        /// Consider refactoring if true async flow is critical here.
        /// </summary>
        /// <param name="CurrentTestType">The test type being considered.</param>
        /// <returns>True if prerequisites are met, false otherwise.</returns>
        public async Task<bool> DoesPassPreviousTest(clsTestType.enTestType CurrentTestType) // Sync signature retained
        {
            // This method becomes tricky with async. Calling .Result or .GetAwaiter().GetResult()
            // inside can lead to deadlocks in some environments (like UI threads).
            // For now, keeping it sync means it might block if the called async methods block.
            // A better approach would be to make this async too and handle the flow accordingly.

            switch (CurrentTestType)
            {
                case clsTestType.enTestType.VisionTest:
                    return true; // No previous test required

                case clsTestType.enTestType.WrittenTest:
                    // BLOCKING CALL - Risk of deadlock!
                    return await DoesPassTestTypeAsync(clsTestType.enTestType.VisionTest);


                case clsTestType.enTestType.StreetTest:
                    // BLOCKING CALL - Risk of deadlock!
                    return await DoesPassTestTypeAsync(clsTestType.enTestType.WrittenTest);

                default:
                    return false;
            }

        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if the latest test of a specific type was passed for a given application ID.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning true if passed, false otherwise.</returns>
        public static async Task<bool> DoesPassTestTypeAsync(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestType) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.DoesPassTestTypeAsync(LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if the applicant attended any test of a specific type.
        /// </summary>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning true if attended, false otherwise.</returns>
        public async Task<bool> DoesAttendTestTypeAsync(clsTestType.enTestType TestType) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.DoesAttendTestTypeAsync(this.LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the total number of trials for a specific test type.
        /// </summary>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning the total trials (byte).</returns>
        public async Task<byte> TotalTrialsPerTestAsync(clsTestType.enTestType TestType) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.TotalTrialsPerTestAsync(this.LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the total number of trials for a specific test type and application ID.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning the total trials (byte).</returns>
        public static async Task<byte> TotalTrialsPerTestAsync(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestType) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.TotalTrialsPerTestAsync(LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if the applicant attended any test of a specific type.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning true if attended, false otherwise.</returns>
        public static async Task<bool> AttendedTestAsync(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestType) // Async suffix
        {
            // Check if trials > 0 by calling the async TotalTrials method
            return (await TotalTrialsPerTestAsync(LocalDrivingLicenseApplicationID, TestType) > 0);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if the applicant attended any test of a specific type for the current application.
        /// </summary>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning true if attended, false otherwise.</returns>
        public async Task<bool> AttendedTestAsync(clsTestType.enTestType TestType) // Async suffix
        {
            // Check if trials > 0 by calling the async TotalTrials method
            return (await TotalTrialsPerTestAsync(TestType) > 0);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if there is an active scheduled test appointment for a specific test type.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning true if an active appointment exists, false otherwise.</returns>
        public static async Task<bool> IsThereAnActiveScheduledTestAsync(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestType) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.IsThereAnActiveScheduledTestAsync(LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if there is an active scheduled test appointment for a specific test type for the current application.
        /// </summary>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning true if an active appointment exists, false otherwise.</returns>
        public async Task<bool> IsThereAnActiveScheduledTestAsync(clsTestType.enTestType TestType) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLocalDrivingLicenseApplicationData.IsThereAnActiveScheduledTestAsync(this.LocalDrivingLicenseApplicationID, (int)TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the last test taken for a specific test type.
        /// Assumes clsTest.FindLastTestPerPersonAndLicenseClassAsync exists.
        /// </summary>
        /// <param name="TestType">The type of test.</param>
        /// <returns>Task returning the clsTest object or null.</returns>
        public async Task<clsTest> GetLastTestPerTestTypeAsync(clsTestType.enTestType TestType)
        {
            // ASSUMPTION: clsTest has a sync version of this method
            return  await clsTest.FindLastTestPerPersonAndLicenseClassAYNC(this.ApplicantPersonID, this.LicenseClassID, TestType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the count of passed tests for the current application.
        /// Assumes clsTest.GetPassedTestCountAsync exists.
        /// </summary>
        /// <returns>Task returning the count of passed tests (byte).</returns>
        public async Task<byte> GetPassedTestCountAsync() // Async suffix
        {
            // ASSUMPTION: clsTest has an async version of this method
            return await clsTest.GetPassedTestCountAsync(this.LocalDrivingLicenseApplicationID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the count of passed tests for a specific application ID.
        /// Assumes clsTest.GetPassedTestCountAsync exists.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <returns>Task returning the count of passed tests (byte).</returns>
        public static async Task<byte> GetPassedTestCountAsync(int LocalDrivingLicenseApplicationID) // Async suffix
        {
            // ASSUMPTION: clsTest has an async version of this method
            return await clsTest.GetPassedTestCountAsync(LocalDrivingLicenseApplicationID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if all required tests have been passed for the current application.
        /// Assumes clsTest.PassedAllTestsAsync exists.
        /// </summary>
        /// <returns>Task returning true if all tests passed, false otherwise.</returns>
        public async Task<bool> PassedAllTestsAsync() // Async suffix
        {
            // ASSUMPTION: clsTest has an async version of this method
            return await clsTest.PassedAllTestsAsync(this.LocalDrivingLicenseApplicationID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if all required tests have been passed for a specific application ID.
        /// Assumes clsTest.PassedAllTestsAsync exists.
        /// </summary>
        /// <param name="LocalDrivingLicenseApplicationID">The application ID.</param>
        /// <returns>Task returning true if all tests passed, false otherwise.</returns>
        public static async Task<bool> PassedAllTestsAsync(int LocalDrivingLicenseApplicationID) // Async suffix
        {
            // ASSUMPTION: clsTest has an async version of this method
            return await clsTest.PassedAllTestsAsync(LocalDrivingLicenseApplicationID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY issues a license for the first time for this application.
        /// Creates a Driver record if one doesn't exist, then creates a License record.
        /// Assumes clsDriver.FindByPersonID remains sync (or becomes async), clsDriver.SaveAsync exists,
        /// clsLicense.SaveAsync exists, and base SetCompleteAsync exists.
        /// </summary>
        /// <param name="Notes">Notes for the new license.</param>
        /// <param name="IssueingByUserID">The ID of the user issuing the license.</param> // Renamed for clarity
        /// <returns>Task returning the new LicenseID, or -1 on failure.</returns>
        public async Task<int> IssueLicenseForTheFirtsTimeAsync(string Notes, int IssueingByUserID) // Async suffix, renamed param
        {
            int driverID = -1;

            // Find driver (assuming sync Find is acceptable or refactored to async)
            clsDriver driver = await clsDriver.FindByDriverID(this.ApplicantPersonID); // Assuming clsDriver.Find is still sync

            if (driver == null)
            {
                // Create new driver if not found
                driver = new clsDriver();
                driver.PersonID = this.ApplicantPersonID;
                driver.CreatedByUserID = IssueingByUserID; // Use renamed parameter

                // ASSUMPTION: clsDriver.SaveAsync exists
                if (await driver.SaveAsync())
                {
                    driverID = driver.DriverID;
                }
                else
                {
                    return -1; // Failed to save driver
                }
            }
            else
            {
                driverID = driver.DriverID;
            }

            // Create the new license
            clsLicense license = new clsLicense();
            license.ApplicationID = this.ApplicationID;
            license.DriverID = driverID;
            license.LicenseClass = this.LicenseClassID; // Use property directly
            license.IssueDate = DateTime.Now;
            license.ExpirationDate = DateTime.Now.AddYears(this.LicenseClassInfo.DefaultValidityLength);
            license.Notes = Notes;
            license.PaidFees = this.LicenseClassInfo.ClassFees;
            license.IsActive = true;
            license.IssueReason = clsLicense.enIssueReason.FirstTime;
            license.CreatedByUserID = IssueingByUserID; // Use renamed parameter

            // ASSUMPTION: clsLicense.SaveAsync exists
            if (await license.SaveAsync())
            {
                // Set the application as complete
                // ASSUMPTION: base.SetCompleteAsync() exists (inherited from clsApplication being refactored)
                await this.SetCompleteAsync();
                return license.LicenseID;
            }
            else
            {
                return -1; // Failed to save license
            }
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if a license has been issued for this application.
        /// </summary>
        /// <returns>Task returning true if an active license exists, false otherwise.</returns>
        public async Task<bool> IsLicenseIssuedAsync() // Async suffix
        {
            // Calls the async GetActiveLicenseIDAsync method
            return (await GetActiveLicenseIDAsync() != -1);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the ID of the active license associated with the applicant and license class of this application.
        /// Assumes clsLicense.GetActiveLicenseIDByPersonIDAsync exists.
        /// </summary>
        /// <returns>Task returning the active LicenseID, or -1 if none found.</returns>
        public async Task<int> GetActiveLicenseIDAsync() // Async suffix
        {
            // ASSUMPTION: clsLicense has an async version of this method
            return await clsLicense.GetActiveLicenseIDByPersonIDAsync(this.ApplicantPersonID, this.LicenseClassID);
        }

    } // End Class clsLocalDrivingLicenseApplication
} // End Namespace