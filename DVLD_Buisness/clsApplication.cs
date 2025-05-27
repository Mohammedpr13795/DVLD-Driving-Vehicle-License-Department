using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

// Assuming clsPerson, clsUser, clsApplicationType exist and are accessible
// using DVLD_Buisness; // If they are in this namespace

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents an Application entity and handles related business logic.
    /// FindBaseApplication method is ASYNCHRONOUSLYLY.
    /// Save, Delete, Cancel, SetComplete, and Exists/GetActiveID methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsApplication
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enum enApplicationType
        {
            NewDrivingLicense = 1, RenewDrivingLicense = 2, ReplaceLostDrivingLicense = 3,
            ReplaceDamagedDrivingLicense = 4, ReleaseDetainedDrivingLicsense = 5, NewInternationalLicense = 6, RetakeTest = 7
        };
        public enum enApplicationStatus { New = 1, Cancelled = 2, Completed = 3 };

        public enMode Mode  = enMode.AddNew; 

        // Properties
        public int ApplicationID { get; set; }
        public int ApplicantPersonID { get; set; }

        // Consider loading PersonInfo aAASYNCHRONOUSLYLYly or on demand if it's slow
        public clsPerson PersonInfo { get;  set; } // Loaded in Find/Constructor

        public string ApplicantFullName => PersonInfo?.FullName ?? "N/A"; // Use null-conditional access

        public DateTime ApplicationDate { get; set; }
        public int ApplicationTypeID { get; set; }

        // Consider loading ApplicationTypeInfo aAASYNCHRONOUSLYLYly or on demand
        public clsApplicationType ApplicationTypeInfo { get;  set; } // Loaded in Find/Constructor

        public enApplicationStatus ApplicationStatus { get; set; }
        public string StatusText
        {
            get
            {
                switch (ApplicationStatus)
                {
                    case enApplicationStatus.New: return "New";
                    case enApplicationStatus.Cancelled: return "Cancelled";
                    case enApplicationStatus.Completed: return "Completed";
                    default: return "Unknown";
                }
            }
        }
        public DateTime LastStatusDate { get; set; }
        public float PaidFees { get; set; }
        public int CreatedByUserID { get; set; }

        // Consider loading CreatedByUserInfo aAASYNCHRONOUSLYLYly or on demand
        public clsUser CreatedByUserInfo { get; set; } // Loaded in Find/Constructor


        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsApplication()
        {
            this.ApplicationID = -1;
            this.ApplicantPersonID = -1;
            this.ApplicationDate = DateTime.Now;
            this.ApplicationTypeID = -1;
            this.ApplicationStatus = enApplicationStatus.New;
            this.LastStatusDate = DateTime.Now;
            this.PaidFees = 0;
            this.CreatedByUserID = -1;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by FindBaseApplication.
        /// </summary>
        private clsApplication(int applicationID, int applicantPersonID,
            DateTime applicationDate, int applicationTypeID,
             enApplicationStatus applicationStatus, DateTime lastStatusDate,
             float paidFees, int createdByUserID , clsPerson PersonInfo , clsApplicationType ApplicationTypeInfo , clsUser CreatedByUserInfo)
        {
            this.ApplicationID = applicationID;
            this.ApplicantPersonID = applicantPersonID;
            this.ApplicationDate = applicationDate;
            this.ApplicationTypeID = applicationTypeID;
            this.ApplicationStatus = applicationStatus;
            this.LastStatusDate = lastStatusDate;
            this.PaidFees = paidFees;
            this.CreatedByUserID = createdByUserID;

            this.PersonInfo = PersonInfo;
            this.ApplicationTypeInfo = ApplicationTypeInfo;
            this.CreatedByUserInfo = CreatedByUserInfo;

            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new application data via DAL.
        /// </summary>
        private async Task<bool> _AddNewApplicationAsync()
        {
            // Call the ASYNC DAL method
            this.ApplicationID = await clsApplicationData.AddNewApplicationAsync(
                this.ApplicantPersonID, this.ApplicationDate,
                this.ApplicationTypeID, (byte)this.ApplicationStatus, // Cast enum to byte
                this.LastStatusDate, this.PaidFees, this.CreatedByUserID);

            return (this.ApplicationID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the application data via DAL.
        /// </summary>
        private async Task<bool> _UpdateApplicationAsync()
        {
            // Call the ASYNC DAL method
            return await clsApplicationData.UpdateApplicationAsync(
                this.ApplicationID, this.ApplicantPersonID, this.ApplicationDate,
                this.ApplicationTypeID, (byte)this.ApplicationStatus, // Cast enum to byte
                this.LastStatusDate, this.PaidFees, this.CreatedByUserID);
        }


        /// <summary>
        /// AAASYNCHRONOUSLYLYLY finds an application by ApplicationID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="ApplicationIDParam">The ID to search for.</param>
        /// <returns>A clsApplication object if found; otherwise, null.</returns>
        public static async Task<clsApplication> FindBaseApplicationAsync(int ApplicationIDParam)
        {
            bool IsFound;
            int ApplicantPersonID = -1;
            DateTime ApplicationDate = DateTime.MinValue; int ApplicationTypeID = -1;
            byte ApplicationStatusByte = 1; DateTime LastStatusDate = DateTime.MinValue;
            float PaidFees = 0; int CreatedByUserID = -1;



            (IsFound, ApplicantPersonID, ApplicationDate, ApplicationTypeID, ApplicationStatusByte,
                LastStatusDate, PaidFees, CreatedByUserID) = await clsApplicationData.GetApplicationInfoByIDAsync(ApplicationIDParam);



            if (IsFound)
            // Use the private constructor
            {
                clsPerson PersonInfo = await clsPerson.FindAsync(ApplicantPersonID);
                clsApplicationType ApplicationTypeInfo = await clsApplicationType.FindAsync(ApplicationTypeID); 
                clsUser CreatedByUserInfo = await clsUser.FindByUserIDAsync(CreatedByUserID);

                return new clsApplication(ApplicationIDParam, ApplicantPersonID,
                                     ApplicationDate, ApplicationTypeID,
                                    (enApplicationStatus)ApplicationStatusByte, LastStatusDate, // Cast byte to enum
                                     PaidFees, CreatedByUserID , PersonInfo , ApplicationTypeInfo , CreatedByUserInfo);
            }
            else
                return null;
        }
        /// <summary>
        /// AAASYNCHRONOUSLYLYLY cancels the current application by updating its status via DAL.
        /// </summary>
        /// <returns>Task returning true if successful, false otherwise.</returns>




        public async Task<bool> CancelAsync() // Async method
        {
            // Call the ASYNC DAL method to update status
            return await clsApplicationData.UpdateStatusAsync(this.ApplicationID, (byte)enApplicationStatus.Cancelled);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY sets the current application to complete via DAL.
        /// </summary>
        /// <returns>Task returning true if successful, false otherwise.</returns>
        public async Task<bool> SetCompleteAsync() // Async method
        {
            // Call the ASYNC DAL method to update status
            return await clsApplicationData.UpdateStatusAsync(this.ApplicationID, (byte)enApplicationStatus.Completed);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current application object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async method
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    // Await the ASYNC private add method
                    if (await _AddNewApplicationAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    // Await the ASYNC private update method
                    return await _UpdateApplicationAsync();
            }
            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY deletes the current application object via DAL.
        /// </summary>
        /// <returns>Task returning true if successful, false otherwise.</returns>
        public async Task<bool> DeleteAsync() // Async method
        {
            // Call the ASYNC DAL method
            return await clsApplicationData.DeleteApplicationAsync(this.ApplicationID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if an application exists by ApplicationID via DAL.
        /// </summary>
        /// <param name="ApplicationID">The ID to check.</param>
        /// <returns>Task returning true if exists, false otherwise.</returns>
        public static async Task<bool> ApplicationExistsAsync(int ApplicationID) // Async method, renamed
        {
            // Call the ASYNC DAL method
            return await clsApplicationData.ApplicationExistsAsync(ApplicationID);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY checks if a person has an active application of a specific type.
        /// Calls the AASYNCHRONOUSLYLY (blocking) DAL method.
        /// </summary>
        /// <param name="PersonID">Person ID.</param>
        /// <param name="ApplicationTypeID">Application Type ID.</param>
        /// <returns>True if an active application exists, false otherwise.</returns>
        public static async Task<bool> DoesPersonHaveActiveApplication(int PersonID, int ApplicationTypeID) 
        {
            // Calls the AASYNCHRONOUSLYLY (blocking) DAL method
            return await clsApplicationData.DoesPersonHaveActiveApplication(PersonID, ApplicationTypeID);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY checks if the current application's applicant has an active application of a specific type.
        /// </summary>
        /// <param name="ApplicationTypeID">Application Type ID.</param>
        /// <returns>True if an active application exists, false otherwise.</returns>
        public async Task<bool> DoesPersonHaveActiveApplication(int ApplicationTypeID)
        {
            // Calls the static AASYNCHRONOUSLYLY version
            return await DoesPersonHaveActiveApplication(this.ApplicantPersonID, ApplicationTypeID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the active application ID for a specific person and application type.
        /// </summary>
        /// <param name="PersonID">Person ID.</param>
        /// <param name="AppType">Application Type Enum.</param>
        /// <returns>Task returning the active ApplicationID or -1 if not found.</returns>
        public static async Task<int> GetActiveApplicationIDAsync(int PersonID, enApplicationType AppType) // Async method
        {
            // Call the ASYNC DAL method
            return await clsApplicationData.GetActiveApplicationIDAsync(PersonID, (int)AppType);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the active application ID for a specific person, type, and license class.
        /// </summary>
        /// <returns>Task returning the active ApplicationID or -1 if not found.</returns>
        public static async Task<int> GetActiveApplicationIDForLicenseClassAsync(int PersonID, enApplicationType AppType, int LicenseClassID) // Async method
        {
            // Call the ASYNC DAL method
            return await clsApplicationData.GetActiveApplicationIDForLicenseClassAsync(PersonID, (int)AppType, LicenseClassID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the active application ID for the current application's applicant and a specific type.
        /// </summary>
        /// <param name="AppType">Application Type Enum.</param>
        /// <returns>Task returning the active ApplicationID or -1 if not found.</returns>
        public async Task<int> GetActiveApplicationIDAsync(enApplicationType AppType) // Async instance method
        {
            // Calls the static ASYNC version
            return await GetActiveApplicationIDAsync(this.ApplicantPersonID, AppType);
        }

    } // End Class clsApplication
} // End Namespace