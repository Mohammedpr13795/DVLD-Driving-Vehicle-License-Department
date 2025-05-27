using System;
using System.Data;
using System.IO;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

// Assuming clsApplication, clsDriver, clsUser are accessible
// using DVLD_Buisness;

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents an International Driver's License, inheriting from clsApplication.
    /// Handles specific logic for international licenses.
    /// Find method is AASYNCHRONOUSLYLY. Save and static Get methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsInternationalLicense : clsApplication // Inherits from clsApplication
    {
        // Mode is inherited
        // public enum enMode { AddNew = 0, Update = 1 };
        // public new enMode Mode = enMode.AddNew;

        // Properties specific to InternationalLicense
        public clsDriver DriverInfo { get; private set; } // Loaded in Find/Constructor
        public int InternationalLicenseID { get; set; }
        public int DriverID { get; set; }
        public int IssuedUsingLocalLicenseID { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        // CreatedByUserID and ApplicationID are inherited

        public clsInternationalLicense()
        {
            //here we set the applicaiton type to New International License.
            this.ApplicationTypeID = (int)clsApplication.enApplicationType.NewInternationalLicense;
            InternationalLicenseID = -1;
            ApplicationID = -1;
            DriverID = -1;
            IssuedUsingLocalLicenseID = -1;
            IssueDate = DateTime.Now;
            ExpirationDate = DateTime.Now;
            IsActive = true;
            CreatedByUserID = -1;

            Mode = enMode.AddNew;
        }


        /// <summary>
        /// Default constructor for AddNew mode. Sets ApplicationTypeID.
        /// </summary>
        private clsInternationalLicense(int ApplicationID, int ApplicantPersonID,
            DateTime ApplicationDate,
             enApplicationStatus ApplicationStatus, DateTime LastStatusDate,
             float PaidFees, int CreatedByUserID,
             int InternationalLicenseID, int DriverID, int IssuedUsingLocalLicenseID,
            DateTime IssueDate, DateTime ExpirationDate, bool IsActive, clsDriver DriverInfo)

        {
            //this is for the base clase
            base.ApplicationID = ApplicationID;
            base.ApplicantPersonID = ApplicantPersonID;
            base.ApplicationDate = ApplicationDate;
            base.ApplicationTypeID = (int)clsApplication.enApplicationType.NewInternationalLicense;
            base.ApplicationStatus = ApplicationStatus;
            base.LastStatusDate = LastStatusDate;
            base.PaidFees = PaidFees;
            base.CreatedByUserID = CreatedByUserID;

            this.InternationalLicenseID = InternationalLicenseID;
            this.ApplicationID = ApplicationID;
            this.DriverID = DriverID;
            this.IssuedUsingLocalLicenseID = IssuedUsingLocalLicenseID;
            this.IssueDate = IssueDate;
            this.ExpirationDate = ExpirationDate;
            this.IsActive = IsActive;
            this.CreatedByUserID = CreatedByUserID;

            this.DriverInfo = DriverInfo;

            Mode = enMode.Update;
        }


        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new international license data via DAL.
        /// </summary>
        private async Task<bool> _AddNewInternationalLicenseAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.InternationalLicenseID = await clsInternationalLicenseData.AddNewInternationalLicenseAsync(
                base.ApplicationID, // Use base property
                this.DriverID, this.IssuedUsingLocalLicenseID, this.IssueDate,
                this.ExpirationDate, this.IsActive, base.CreatedByUserID); // Use base property

            return (this.InternationalLicenseID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the international license data via DAL.
        /// </summary>
        private async Task<bool> _UpdateInternationalLicenseAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsInternationalLicenseData.UpdateInternationalLicenseAsync(
                this.InternationalLicenseID, base.ApplicationID, this.DriverID, this.IssuedUsingLocalLicenseID,
                this.IssueDate, this.ExpirationDate, this.IsActive, base.CreatedByUserID);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds an international license by its ID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="InternationalLicenseIDParam">The ID to search for.</param>
        /// <returns>A clsInternationalLicense object if found; otherwise, null.</returns>
        public static async Task<clsInternationalLicense> FindAsync(int InternationalLicenseIDParam) // Sync signature
        {
            // Declare variables for ref parameters
            int ApplicationID = -1, DriverID = -1, IssuedUsingLocalLicenseID = -1, CreatedByUserID = -1;
            DateTime IssueDate = DateTime.MinValue, ExpirationDate = DateTime.MinValue;
            bool IsActive = false, IsFound = false ;

            (IsFound, ApplicationID, DriverID, IssuedUsingLocalLicenseID,
                    IssueDate, ExpirationDate, IsActive, CreatedByUserID) = await clsInternationalLicenseData.GetInternationalLicenseInfoByIDAsync(InternationalLicenseIDParam);
            // Call ASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                // Find base application data (AASYNCHRONOUSLYLYLY)
                clsApplication BaseApplication = await clsApplication.FindBaseApplicationAsync(ApplicationID); // Assumes sync Find

                if (BaseApplication != null)
                {
                    clsDriver DriverInfo = await clsDriver.FindByDriverID(DriverID);
                    // Use private constructor to create the derived object
                    return new clsInternationalLicense(
                        BaseApplication.ApplicationID, BaseApplication.ApplicantPersonID, BaseApplication.ApplicationDate,
                        BaseApplication.ApplicationStatus, BaseApplication.LastStatusDate, BaseApplication.PaidFees, BaseApplication.CreatedByUserID,
                        InternationalLicenseIDParam, DriverID, IssuedUsingLocalLicenseID,
                        IssueDate, ExpirationDate, IsActive , DriverInfo);
                }
                else
                {
                    clsEventLogger.LogEvent($"Inconsistency: InternationalLicense {InternationalLicenseIDParam} found, but base Application {ApplicationID} not found.", System.Diagnostics.EventLogEntryType.Error);
                    return null;
                }
            }
            else
                return null; // International License not found
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all international licenses.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        public static async Task<DataTable> GetAllInternationalLicensesAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsInternationalLicenseData.GetAllInternationalLicensesAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current international license object.
        /// Saves the base application first, then the international license details.
        /// Assumes base.SaveAsync() exists.
        /// </summary>
        /// <returns>A Task returning true if both saves were successful; false otherwise.</returns>
        public new async Task<bool> SaveAsync() // Async suffix, use 'new' or override
        {
            // Set the base class mode
            base.Mode = (clsApplication.enMode)Mode; // Make sure Mode property exists/accessible in base

            // Await the base class save method first
            bool baseSaveSuccess = await base.SaveAsync(); // ASSUMES base.SaveAsync() exists

            if (!baseSaveSuccess) { return false; }

            // Now save the international license part
            switch (this.Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewInternationalLicenseAsync())
                    {
                        this.Mode = enMode.Update; // Update derived mode
                        base.Mode = clsApplication.enMode.Update; // Update base mode
                        return true;
                    }
                    else { return false; } // Consider rollback?

                case enMode.Update:
                    return await _UpdateInternationalLicenseAsync();
            }
            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the active International License ID for a specific driver.
        /// </summary>
        /// <param name="DriverID">The ID of the driver.</param>
        /// <returns>Task returning the active InternationalLicenseID or -1 if none found.</returns>
        public static async Task<int> GetActiveInternationalLicenseIDByDriverIDAsync(int DriverID) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsInternationalLicenseData.GetActiveInternationalLicenseIDByDriverIDAsync(DriverID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable of international licenses for a specific driver.
        /// </summary>
        /// <param name="DriverID">The ID of the driver.</param>
        /// <returns>Task returning a DataTable of international licenses.</returns>
        public static async Task<DataTable> GetDriverInternationalLicensesAsync(int DriverID) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsInternationalLicenseData.GetDriverInternationalLicensesAsync(DriverID);
        }

        // --- Original AASYNCHRONOUSLYLY Methods (Consider replacing calls with Async versions) ---
        /*
        public static DataTable GetAllInternationalLicenses() => GetAllInternationalLicensesAsync().GetAwaiter().GetResult();
        public bool Save() => SaveAsync().GetAwaiter().GetResult();
        public static int GetActiveInternationalLicenseIDByDriverID(int DriverID) => GetActiveInternationalLicenseIDByDriverIDAsync(DriverID).GetAwaiter().GetResult();
        public static DataTable GetDriverInternationalLicenses(int DriverID) => GetDriverInternationalLicensesAsync(DriverID).GetAwaiter().GetResult();
        */

    } // End Class clsInternationalLicense
} // End Namespace