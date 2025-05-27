using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

// Assuming clsUser and clsApplication exist and are accessible
// using DVLD_Buisness;

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Detained License record and handles related business logic.
    /// Find methods are AASYNCHRONOUSLYLY.
    /// Save, GetAll, Release, and IsLicenseDetained methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsDetainedLicense
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties
        public int DetainID { get; set; }
        public int LicenseID { get; set; }
        public DateTime DetainDate { get; set; }
        public float FineFees { get; set; }
        public int CreatedByUserID { get; set; }
        public clsUser CreatedByUserInfo { get; private set; } // Loaded sync in constructor/Find
        public bool IsReleased { get; private set; } // Make setter private, controlled by Release method
        public DateTime ReleaseDate { get; private set; } // Make setter private
        public int ReleasedByUserID { get; private set; } // Make setter private
        public clsUser ReleasedByUserInfo { get; private set; } // Make setter private
        public int ReleaseApplicationID { get; private set; } // Make setter private


        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsDetainedLicense()
        {
            this.DetainID = -1;
            this.LicenseID = -1;
            this.DetainDate = DateTime.MinValue; // Should be set before save
            this.FineFees = 0;
            this.CreatedByUserID = -1; // Should be set before save
            this.IsReleased = false;
            this.ReleaseDate = DateTime.MinValue; // Use MinValue for not set
            this.ReleasedByUserID = -1;
            this.ReleaseApplicationID = -1;
            this.CreatedByUserInfo = null;
            this.ReleasedByUserInfo = null;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find methods. Corrected loading of ReleasedByUserInfo.
        /// </summary>
        private clsDetainedLicense(int detainID,
            int licenseID, DateTime detainDate,
            float fineFees, int createdByUserID,
            bool isReleased, DateTime releaseDate,
            int releasedByUserID, int releaseApplicationID , clsUser CreatedByUserInfo , clsUser ReleasedByUserInfo)
        {
            this.DetainID = detainID;
            this.LicenseID = licenseID;
            this.DetainDate = detainDate;
            this.FineFees = fineFees;
            this.CreatedByUserID = createdByUserID;
            this.IsReleased = isReleased;
            this.ReleaseDate = releaseDate; // Will be MinValue if not released
            this.ReleasedByUserID = releasedByUserID; // Will be -1 if not released
            this.ReleaseApplicationID = releaseApplicationID; // Will be -1 if not released

            this.CreatedByUserInfo = CreatedByUserInfo;

            if (this.IsReleased && this.ReleasedByUserID != -1)
            {
                // Corrected: Use FindByUserID for ReleasedByUserInfo
                this.ReleasedByUserInfo = ReleasedByUserInfo;
            }
            else
            {
                this.ReleasedByUserInfo = null;
            }

            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new detained license data via DAL.
        /// </summary>
        private async Task<bool> _AddNewDetainedLicenseAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.DetainID = await clsDetainedLicenseData.AddNewDetainedLicenseAsync(
                this.LicenseID, this.DetainDate, this.FineFees, this.CreatedByUserID);

            return (this.DetainID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the detained license basic data via DAL.
        /// Note: Not typically used, as release is handled separately.
        /// </summary>
        private async Task<bool> _UpdateDetainedLicenseAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsDetainedLicenseData.UpdateDetainedLicenseAsync(
                this.DetainID, this.LicenseID, this.DetainDate, this.FineFees, this.CreatedByUserID);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a detained license record by DetainID.
        /// </summary>
        /// <param name="DetainIDParam">The DetainID to search for.</param>
        /// <returns>A clsDetainedLicense object if found; otherwise, null.</returns>
        public static async Task<clsDetainedLicense> FindAsync(int DetainIDParam) // Sync signature
        {
            // Declare variables for ref parameters
            int LicenseID = -1, CreatedByUserID = -1, ReleasedByUserID = -1, ReleaseApplicationID = -1;
            DateTime DetainDate = DateTime.MinValue, ReleaseDate = DateTime.MinValue;
            float FineFees = 0;
            bool IsReleased = false ,IsFound = false;
            ( IsFound ,  LicenseID, DetainDate, FineFees, CreatedByUserID, IsReleased,
             ReleaseDate, ReleasedByUserID, ReleaseApplicationID) = await clsDetainedLicenseData.GetDetainedLicenseInfoByIDAsync(DetainIDParam);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                clsUser CreatedByUserInfo = await clsUser.FindByUserIDAsync(CreatedByUserID);
                clsUser ReleasedByUserInfo = await clsUser.FindByUserIDAsync(ReleasedByUserID);

                // Use private constructor
                return new clsDetainedLicense(DetainIDParam, LicenseID, DetainDate, FineFees, CreatedByUserID,
                                             IsReleased, ReleaseDate, ReleasedByUserID, ReleaseApplicationID , CreatedByUserInfo , ReleasedByUserInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds the latest detention record for a specific LicenseID.
        /// </summary>
        /// <param name="LicenseIDParam">The LicenseID to search for.</param>
        /// <returns>A clsDetainedLicense object if found; otherwise, null.</returns>
        public static async Task<clsDetainedLicense> FindByLicenseID(int LicenseIDParam) // Sync signature
        {
            // Declare variables for ref parameters
            int DetainID = -1, CreatedByUserID = -1, ReleasedByUserID = -1, ReleaseApplicationID = -1;
            DateTime DetainDate = DateTime.MinValue, ReleaseDate = DateTime.MinValue;
            float FineFees = 0;
            bool IsReleased = false;

            // Call AASYNCHRONOUSLYLY DAL method
            if (clsDetainedLicenseData.GetDetainedLicenseInfoByLicenseID(LicenseIDParam,
                ref DetainID, ref DetainDate, ref FineFees, ref CreatedByUserID,
                ref IsReleased, ref ReleaseDate, ref ReleasedByUserID, ref ReleaseApplicationID))
            {

                clsUser CreatedByUserInfo = await clsUser.FindByUserIDAsync(CreatedByUserID);
                clsUser ReleasedByUserInfo = await clsUser.FindByUserIDAsync(ReleasedByUserID);
                // Use private constructor
                return new clsDetainedLicense(DetainID, LicenseIDParam, DetainDate, FineFees, CreatedByUserID,
                                             IsReleased, ReleaseDate, ReleasedByUserID, ReleaseApplicationID , CreatedByUserInfo , ReleasedByUserInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all detained licenses.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        public static async Task<DataTable> GetAllDetainedLicensesAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsDetainedLicenseData.GetAllDetainedLicensesAsync();
        }

        public async Task<bool> ReleaseDetainedLicenseAsync(int ReleasedByUserID, int ReleaseApplicationID)
        {
            return await clsDetainedLicenseData.ReleaseDetainedLicenseAsync(this.DetainID,
                   ReleasedByUserID, ReleaseApplicationID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current detained license object (Adds New or Updates basic info).
        /// Important: Logic to deactivate the associated License should precede calling SaveAsync in AddNew mode.
        /// Use ReleaseAsync to update release status.
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async suffix
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    // **ACTION REQUIRED:** Before calling Add, ensure the license (this.LicenseID)
                    // is marked as inactive in the Licenses table. This usually involves:
                    // 1. Finding the clsLicense object.
                    // 2. Calling its DeactivateAsync() method.
                    // 3. Handling potential errors from deactivation *before* proceeding.
                    // Example (pseudo-code):
                    // clsLicense licenseToDeactivate = await clsLicense.FindLicenseByIDAsync(this.LicenseID); // Assuming async Find exists
                    // if (licenseToDeactivate == null || !await licenseToDeactivate.DeactivateAsync()) { return false; /* Failed to deactivate */ }

                    if (await _AddNewDetainedLicenseAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    // Only updates basic info like FineFees, DetainDate, CreatedByUserID
                    return await _UpdateDetainedLicenseAsync();
            }
            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if a specific license is currently detained (IsReleased=0).
        /// </summary>
        /// <param name="LicenseID">The LicenseID to check.</param>
        /// <returns>Task returning true if detained, false otherwise.</returns>
        public static async Task<bool> IsLicenseDetainedAsync(int LicenseID) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsDetainedLicenseData.IsLicenseDetainedAsync(LicenseID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY releases the current detained license instance.
        /// Updates the record in DetainedLicenses and reactivates the license in Licenses table via SP.
        /// </summary>
        /// <param name="ReleasedByUser">The User object performing the release.</param> // Changed parameter type
        /// <param name="ReleaseApp">The Application object related to the release.</param> // Changed parameter type
        /// <returns>Task returning true if release was successful, false otherwise.</returns>
        public async Task<bool> ReleaseAsync(clsUser ReleasedByUser, clsApplication ReleaseApp) // Async suffix, better parameters
        {
            // Validate inputs
            if (this.DetainID == -1 || this.IsReleased)
            {
                // Cannot release a new or already released record
                return false;
            }
            if (ReleasedByUser == null || ReleaseApp == null || ReleasedByUser.UserID <= 0 || ReleaseApp.ApplicationID <= 0)
            {
                // Invalid user or application for release info
                return false;
            }

            // Call the ASYNC DAL method which handles the transaction
            bool success = await clsDetainedLicenseData.ReleaseDetainedLicenseAsync(
                                    this.DetainID,
                                    ReleasedByUser.UserID, // Pass UserID
                                    ReleaseApp.ApplicationID); // Pass ApplicationID

            if (success)
            {
                // Update the object's state upon successful release
                this.IsReleased = true;
                this.ReleasedByUserID = ReleasedByUser.UserID;
                this.ReleasedByUserInfo = ReleasedByUser; // Assign the passed object
                this.ReleaseApplicationID = ReleaseApp.ApplicationID;
                this.ReleaseDate = DateTime.Now; // Set approximate release date (actual time is from GETDATE() in SP)
                this.Mode = enMode.Update; // Ensure mode reflects the change
            }

            return success;
        }


        // --- Original AASYNCHRONOUSLYLY Methods (Consider replacing calls with Async versions) ---
        /*
        public static DataTable GetAllDetainedLicenses() => GetAllDetainedLicensesAsync().GetAwaiter().GetResult();
        public bool Save() => SaveAsync().GetAwaiter().GetResult();
        public static bool IsLicenseDetained(int LicenseID) => IsLicenseDetainedAsync(LicenseID).GetAwaiter().GetResult();

        // AASYNCHRONOUSLYLY Release wrapper is problematic due to needing User/App objects and blocking
        public bool ReleaseDetainedLicense(int ReleasedByUserID, int ReleaseApplicationID)
        {
            // Finding User/App AASYNCHRONOUSLYLYly might block or return stale data if those Finds are async.
             clsUser user = clsUser.FindByUserID(ReleasedByUserID);
             clsApplication app = clsApplication.FindBaseApplication(ReleaseApplicationID);
             if (user == null || app == null) return false;
             try { return ReleaseAsync(user, app).GetAwaiter().GetResult(); }
             catch { return false; }
        }
        */

    } // End Class clsDetainedLicense
} // End Namespace