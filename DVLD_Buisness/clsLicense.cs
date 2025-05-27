using System;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using DVLD_DataAccess; // Reference DAL

// Assuming clsApplication, clsDriver, clsLicenseClass, clsUser, clsDetainedLicense, clsApplicationType exist and follow the established async pattern where appropriate.
// using DVLD_Buisness;

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Driver's License entity. Find is ASYNCHRONOUSLYLY.
    /// Original methods remain ASYNCHRONOUSLYLY (using blocking waits). New Async methods added.
    /// Compatible with C# 7.3. NO LAMBDAS.
    /// </summary>
    public class clsLicense
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enum enIssueReason { FirstTime = 1, Renew = 2, ReplacementForDamaged = 3, ReplacementForLost = 4 };

        public enMode Mode = enMode.AddNew; // Original public field

        // Original Properties
        public int LicenseID { set; get; }
        public int ApplicationID { set; get; }
        public int DriverID { set; get; }
        public int LicenseClass { set; get; }
        public clsLicenseClass LicenseClassInfo { get; private set; } // Keep private set
        public DateTime IssueDate { set; get; }
        public DateTime ExpirationDate { set; get; }
        public string Notes { set; get; }
        public float PaidFees { set; get; }
        public bool IsActive { set; get; }
        public enIssueReason IssueReason { set; get; }
        // Original Getter for IssueReasonText
        public string IssueReasonText
        {
            get { return GetIssueReasonText(this.IssueReason); }
        }
        public Task<bool> IsDetained
        {
            get { return  clsDetainedLicense.IsLicenseDetainedAsync(this.LicenseID); }
        }
        public clsDetainedLicense DetainedInfo { set; get; } // Original public setter
        public int CreatedByUserID { set; get; }
        public clsDriver DriverInfo { get; private set; } // Keep private set

        // IsDetained Property removed, replaced by IsDetainedAsync() method

        /// <summary> Default constructor. </summary>
        public clsLicense()
        {
            this.LicenseID = -1; this.ApplicationID = -1; this.DriverID = -1; this.LicenseClass = -1;
            this.IssueDate = DateTime.MinValue; this.ExpirationDate = DateTime.MinValue; this.Notes = null;
            this.PaidFees = 0; this.IsActive = true; // Defaulting to true based on original _AddNew logic? Review needed.
            this.IssueReason = enIssueReason.FirstTime; this.CreatedByUserID = -1;
            Mode = enMode.AddNew;
            this.DriverInfo = null; this.LicenseClassInfo = null;
            this.DetainedInfo = null;
        }

        /// <summary> Private constructor for loading. </summary>
        private clsLicense(int licenseID, int applicationID, int driverID, int licenseClass,
                           DateTime issueDate, DateTime expirationDate, string notes,
                           float paidFees, bool isActive, byte issueReasonByte, int createdByUserID , clsDriver DriverInfo , 
                           clsLicenseClass LicenseClassInfo , clsDetainedLicense DetainedInfo)
        {
            this.LicenseID = licenseID; this.ApplicationID = applicationID; this.DriverID = driverID;
            this.LicenseClass = licenseClass; this.IssueDate = issueDate; this.ExpirationDate = expirationDate;
            this.Notes = notes; this.PaidFees = paidFees; this.IsActive = isActive;
            this.IssueReason = (enIssueReason)issueReasonByte; this.CreatedByUserID = createdByUserID;
            // Load related objects AAASYNCHRONOUSLYLYly
            // this.ApplicationInfo = clsApplication.FindBaseApplication(applicationID); // Lazy load maybe?
            this.DriverInfo =  DriverInfo; // Assumes sync Find
            this.LicenseClassInfo = LicenseClassInfo; // Assumes sync Find
            this.DetainedInfo = DetainedInfo; // Assumes sync Find
            Mode = enMode.Update;
        }

        /// <summary> Private ASYNC method to add via DAL. </summary>
        private async Task<bool> _AddNewLicenseAsync()
        {
            this.LicenseID = await clsLicenseData.AddNewLicenseAsync(this.ApplicationID, this.DriverID, this.LicenseClass, this.IssueDate, this.ExpirationDate, this.Notes, this.PaidFees, this.IsActive, (byte)this.IssueReason, this.CreatedByUserID);
            return (this.LicenseID != -1);
        }

        /// <summary> Private ASYNC method to update via DAL. </summary>
        private async Task<bool> _UpdateLicenseAsync()
        {
            return await clsLicenseData.UpdateLicenseAsync(this.LicenseID, this.ApplicationID, this.DriverID, this.LicenseClass, this.IssueDate, this.ExpirationDate, this.Notes, this.PaidFees, this.IsActive, (byte)this.IssueReason, this.CreatedByUserID);
        }

        public static async Task<clsLicense> FindAsync(int LicenseID) // Sync
        {
            bool IsFound = false; int ApplicationID = -1, DriverID = -1, LicenseClass = -1, CreatedByUserID = -1;
            DateTime IssueDate = DateTime.MinValue, ExpirationDate = DateTime.MinValue; string Notes = string.Empty; 
            float PaidFees; bool IsActive = false; byte IssueReasonByte = 0;

            (IsFound, ApplicationID, DriverID, LicenseClass,IssueDate, ExpirationDate, Notes, PaidFees, IsActive,IssueReasonByte, CreatedByUserID) 
                    = await clsLicenseData.GetLicenseInfoByIDAsync(LicenseID);
            // Calls Sync DAL
            if (IsFound)
            {
                clsDriver DriverInfo = await clsDriver.FindByDriverID(DriverID); // Assumes sync Find
                clsLicenseClass LicenseClassInfo = await clsLicenseClass.FindAsync(LicenseClass); // Assumes sync Find
                clsDetainedLicense DetainedInfo = await clsDetainedLicense.FindByLicenseID(LicenseID); // Assumes sync Find
                return new clsLicense(LicenseID, ApplicationID, DriverID, LicenseClass, IssueDate, ExpirationDate, Notes, PaidFees, IsActive,
                    IssueReasonByte, CreatedByUserID , DriverInfo , LicenseClassInfo , DetainedInfo);
            }
            else return null;
        }

        public static async Task<DataTable> GetAllLicenses()
        {
            return await clsLicenseData.GetAllLicensesAsync();

        }

        public static async Task<bool> IsLicenseExistByPersonID(int PersonID, int LicenseClassID)
        {
            return (await GetActiveLicenseIDByPersonID(PersonID, LicenseClassID) != -1);
        }
        public static async Task<int> GetActiveLicenseIDByPersonID(int PersonID, int LicenseClassID)
        {

            return await clsLicenseData.GetActiveLicenseIDByPersonIDAsync(PersonID, LicenseClassID);

        }
        public static async Task<DataTable> GetDriverLicenses(int DriverID)
        {
            return await clsLicenseData.GetDriverLicensesAsync(DriverID);
        }
        public Boolean IsLicenseExpired()
        {
            return (this.ExpirationDate < DateTime.Now);
        }
        public async Task<bool> DeactivateCurrentLicenseAsync()
        {
            return (await clsLicenseData.DeactivateLicenseAsync(this.LicenseID));
        }
        public static string GetIssueReasonText(enIssueReason IssueReason) // No change needed
        {
            switch (IssueReason)
            {
                case enIssueReason.FirstTime:
                    return "First Time";
                case enIssueReason.Renew:
                    return "Renew";
                case enIssueReason.ReplacementForDamaged:
                    return "Replacement for Damaged";
                case enIssueReason.ReplacementForLost:
                    return "Replacement for Lost";
                default:
                    return "First Time";
            }
        }

        /// <summary>
        /// AAAASYNCHRONOUSLYLYLY detains the current license.
        /// Deactivates the current license and creates a new detention record.
        /// (Replaces the original AAASYNCHRONOUSLYLY Detain method).
        /// </summary>
        /// <param name="FineFees">The fine amount for the detention.</param>
        /// <param name="CreatedByUserID">The ID of the user performing the detention.</param>
        /// <returns>A Task returning the new DetainID, or -1 on failure.</returns>
        public async Task<int> DetainAsync(float FineFees, int CreatedByUserID) // Changed signature to async Task<int>
        {
            // Create the object AAASYNCHRONOUSLYLYly as before
            clsDetainedLicense detainedLicense = new clsDetainedLicense();
            detainedLicense.LicenseID = this.LicenseID;
            detainedLicense.DetainDate = DateTime.Now;
            detainedLicense.FineFees = FineFees;
            detainedLicense.CreatedByUserID = CreatedByUserID;

            // Call SaveAsync and await the result
            // ASSUMING detainedLicense.SaveAsync() returns Task<bool>
            if (!await detainedLicense.SaveAsync()) // Check the boolean result
            {
                // Optional: Log failure (logging code not added as per request)
                return -1;
            }

            // Return the ID from the successfully saved object
            return detainedLicense.DetainID;
        }

        /// <summary>
        /// AAAASYNCHRONOUSLYLYLY releases a detained license. Creates application, updates statuses.
        /// Returns a tuple indicating success and the new ApplicationID.
        /// (Replaces the original AAASYNCHRONOUSLYLY ReleaseDetainedLicense method using Tuple instead of ref).
        /// This version strictly follows the original logic structure + async/await.
        /// </summary>
        /// <param name="ReleasedByUserID">ID of the user performing the release.</param>
        /// <returns>A Task returning a tuple: (bool Success, int ApplicationID). ApplicationID is -1 on failure.</returns>
        public async Task<(bool Success, int ApplicationID)> ReleaseDetainedLicenseAsync(int ReleasedByUserID) // Changed signature: async Task<Tuple>, removed ref
        {
            int localApplicationID = -1; // Local variable to store the ID

            //First Create Application
            clsApplication Application = new clsApplication(); // Assuming constructor is accessible

            // Assuming DriverInfo is loaded and valid
            // No extra null check added as requested. Assumes DriverInfo is valid.
            Application.ApplicantPersonID = this.DriverInfo.PersonID;
            Application.ApplicationDate = DateTime.Now;
            Application.ApplicationTypeID = (int)clsApplication.enApplicationType.ReleaseDetainedDrivingLicsense;
            Application.ApplicationStatus = clsApplication.enApplicationStatus.Completed;
            Application.LastStatusDate = DateTime.Now;
            // Assuming clsApplicationType.Find is AAASYNCHRONOUSLYLY and returns valid object
            Application.PaidFees = (await clsApplicationType.FindAsync((int)clsApplication.enApplicationType.ReleaseDetainedDrivingLicsense)).Fees;
            Application.CreatedByUserID = ReleasedByUserID;

            // Save Application AAAASYNCHRONOUSLYLYLY
            // ASSUMPTION: Application.SaveAsync() exists and returns Task<bool>
            if (!await Application.SaveAsync()) // Use await, check boolean result
            {
                // Application save failed
                // No logging added as requested
                return (false, -1);
            }




            bool releaseSuccess = await this.DetainedInfo.ReleaseDetainedLicenseAsync(ReleasedByUserID, ApplicationID); // Use await

            // Original code just returned the result of the final DAL call.
            // We return success status and the ApplicationID created.
            if (releaseSuccess)
            {
                // Optionally refresh local state (IsActive, DetainedInfo) if needed by caller immediately after.
                // this.IsActive = true;
                // this.DetainedInfo = clsDetainedLicense.Find(detainedInfoToRelease.DetainID); // Sync Find
                return (true, localApplicationID:Application.ApplicationID);
            }
            else
            {
                return (false, localApplicationID); // Return failure, but include created Application ID
            }
        }


        // --- NEW AAAASYNCHRONOUSLYLY BLL Methods ---

        /// <summary> AAAASYNCHRONOUSLYLYLY saves the license. </summary>
        public async Task<bool> SaveAsync() // New Async
        {
            switch (Mode)
            {
                case enMode.AddNew: if (await _AddNewLicenseAsync()) { Mode = enMode.Update; return true; } else { return false; }
                case enMode.Update: return await _UpdateLicenseAsync();
            }
            return false;
        }

        /// <summary> AAAASYNCHRONOUSLYLYLY gets all licenses. </summary>
        public static async Task<DataTable> GetAllLicensesAsync() // New Async
        { return await clsLicenseData.GetAllLicensesAsync(); }

        /// <summary> AAAASYNCHRONOUSLYLYLY gets licenses for a driver. </summary>
        public static async Task<DataTable> GetDriverLicensesAsync(int DriverID) // New Async
        { return await clsLicenseData.GetDriverLicensesAsync(DriverID); }


        /// <summary> AAAASYNCHRONOUSLYLYLY gets active license ID for person/class. </summary>
        public static async Task<int> GetActiveLicenseIDByPersonIDAsync(int PersonID, int LicenseClassID) // New Async
        { return await clsLicenseData.GetActiveLicenseIDByPersonIDAsync(PersonID, LicenseClassID); }

        /// <summary> AAAASYNCHRONOUSLYLYLY deactivates the current license instance. </summary>
        public async Task<bool> DeactivateAsync() // New Async instance method
        {
            if (this.LicenseID == -1 || !this.IsActive) return false;
            if (await clsLicenseData.DeactivateLicenseAsync(this.LicenseID)) { this.IsActive = false; return true; } else { return false; }
        }

        /// <summary> AAAASYNCHRONOUSLYLYLY deactivates a license by ID. </summary>
        public static async Task<bool> DeactivateLicenseAsync(int LicenseID) // New Static Async method
        { if (LicenseID == -1) return false; return await clsLicenseData.DeactivateLicenseAsync(LicenseID); }

    

        /// <summary> AAAASYNCHRONOUSLYLYLY renews the license. </summary>
        public async Task<clsLicense> RenewLicenseAsync(string Notes, int CreatedByUserID) // New Async
        {
            // Create Application
            clsApplication Application = new clsApplication();
            // ... (Set application properties as in sync version) ...
            Application.ApplicantPersonID = this.DriverInfo.PersonID;
            Application.ApplicationDate = DateTime.Now;
            Application.ApplicationTypeID = (int)clsApplication.enApplicationType.RenewDrivingLicense;
            Application.ApplicationStatus = clsApplication.enApplicationStatus.Completed;
            Application.LastStatusDate = DateTime.Now;
            Application.PaidFees = (await clsApplicationType.FindAsync((int)clsApplication.enApplicationType.RenewDrivingLicense)).Fees; // Assumes sync Find
            Application.CreatedByUserID = CreatedByUserID;

            if (!await Application.SaveAsync()) { return null; } // Use Async Save

            // Create New License
            clsLicense NewLicense = new clsLicense();
            // ... (Set NewLicense properties as in sync version) ...
            NewLicense.ApplicationID = Application.ApplicationID;
            NewLicense.DriverID = this.DriverID;
            NewLicense.LicenseClass = this.LicenseClass;
            NewLicense.IssueDate = DateTime.Now;
            NewLicense.ExpirationDate = DateTime.Now.AddYears(this.LicenseClassInfo.DefaultValidityLength);
            NewLicense.Notes = Notes;
            NewLicense.PaidFees = this.LicenseClassInfo.ClassFees;
            NewLicense.IsActive = true;
            NewLicense.IssueReason = clsLicense.enIssueReason.Renew;
            NewLicense.CreatedByUserID = CreatedByUserID;

            if (!await NewLicense.SaveAsync()) { return null; } // Use Async Save

           await DeactivateCurrentLicenseAsync();

            return NewLicense;
        }

        /// <summary> AAAASYNCHRONOUSLYLYLY replaces the license. </summary>
        public async Task<clsLicense> ReplaceAsync(enIssueReason IssueReason, int CreatedByUserID) // New Async
        {
            // Create Application
            clsApplication Application = new clsApplication();
            // ... (Set application properties as in sync version) ...
            Application.ApplicantPersonID = this.DriverInfo.PersonID;
            Application.ApplicationDate = DateTime.Now;
            Application.ApplicationTypeID = (IssueReason == enIssueReason.ReplacementForDamaged) ? (int)clsApplication.enApplicationType.ReplaceDamagedDrivingLicense : (int)clsApplication.enApplicationType.ReplaceLostDrivingLicense;
            Application.ApplicationStatus = clsApplication.enApplicationStatus.Completed;
            Application.LastStatusDate = DateTime.Now;
            Application.PaidFees = (await clsApplicationType.FindAsync(Application.ApplicationTypeID)).Fees; // Assumes sync Find
            Application.CreatedByUserID = CreatedByUserID;

            if (!await Application.SaveAsync()) { return null; } // Use Async Save

            // Create New License
            clsLicense NewLicense = new clsLicense();
            // ... (Set NewLicense properties as in sync version, keeping expiration date) ...
            NewLicense.ApplicationID = Application.ApplicationID;
            NewLicense.DriverID = this.DriverID;
            NewLicense.LicenseClass = this.LicenseClass;
            NewLicense.IssueDate = DateTime.Now;
            NewLicense.ExpirationDate = this.ExpirationDate; // Keep original expiration
            NewLicense.Notes = this.Notes;
            NewLicense.PaidFees = 0;
            NewLicense.IsActive = true;
            NewLicense.IssueReason = IssueReason;
            NewLicense.CreatedByUserID = CreatedByUserID;

            if (!await NewLicense.SaveAsync()) { return null; } // Use Async Save

            // Deactivate old License AAAASYNCHRONOUSLYLYLY
            await DeactivateCurrentLicenseAsync();

            return NewLicense;
        }

        /// <summary> AAAASYNCHRONOUSLYLYLY checks if the license is detained. </summary>
        public async Task<bool> IsDetainedAsync() // New Async method
        {
            if (this.LicenseID == -1) return false;
            return await clsDetainedLicense.IsLicenseDetainedAsync(this.LicenseID); // Calls static async
        }

    } // End Class clsLicense
} // End Namespace