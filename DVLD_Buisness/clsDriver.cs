using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

// Assuming clsPerson, clsLicense, clsInternationalLicense are accessible
// using DVLD_Buisness;

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Driver entity and handles related business logic.
    /// Find methods are AAASYNCHRONOUSLYLY.
    /// Save, GetAll, and GetLicenses methods are AAAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsDriver
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties
        public clsPerson PersonInfo { get; private set; } // Loaded AAASYNCHRONOUSLYLYly in Find/Constructor
        public int DriverID { get; set; }
        public int PersonID { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime CreatedDate { get; private set; } // Make setter private, set in constructor

        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsDriver()
        {
            this.DriverID = -1;
            this.PersonID = -1;
            this.CreatedByUserID = -1;
            // CreatedDate is set upon successful save or when loading existing record
            this.CreatedDate = DateTime.MinValue;
            this.PersonInfo = null;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find methods.
        /// </summary>
        private clsDriver(int driverID, int personID, int createdByUserID, DateTime createdDate , clsPerson PersonInfo)
        {
            this.DriverID = driverID;
            this.PersonID = personID;
            this.CreatedByUserID = createdByUserID;
            this.CreatedDate = createdDate; // Set the date loaded from DB

            this.PersonInfo = PersonInfo;
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAAASYNCHRONOUSLYLY method to add the new driver data via DAL.
        /// </summary>
        private async Task<bool> _AddNewDriverAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.DriverID = await clsDriverData.AddNewDriverAsync(this.PersonID, this.CreatedByUserID);

            // Note: We don't get the exact CreatedDate back from AddNewDriverAsync easily
            // without modifying the SP/DAL. If needed, call FindByDriverID after successful add,
            // but for now, assume it's set close enough by DateTime.Now in constructor.
            // Or better: Modify SP_AddNewDriver to OUTPUT CreatedDate as well.

            return (this.DriverID != -1);
        }

        /// <summary>
        /// Private AAAASYNCHRONOUSLYLY method to update the driver data via DAL.
        /// </summary>
        private async Task<bool> _UpdateDriverAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsDriverData.UpdateDriverAsync(this.DriverID, this.PersonID, this.CreatedByUserID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY finds a driver by DriverID using the AAASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="DriverIDParam">The ID to search for.</param>
        /// <returns>A clsDriver object if found; otherwise, null.</returns>
        public static async Task<clsDriver> FindByDriverID(int DriverIDParam) // Renamed from FindByDriverID for consistency
        {
            // Declare variables for ref parameters
            (bool IsFound, int PersonID, int CreatedByUserID, DateTime CreatedDate) = await clsDriverData.GetDriverInfoByDriverIDAsync(DriverIDParam);

            // Call ASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                clsPerson PersonInfo = await clsPerson.FindAsync(PersonID);
                // Use private constructor
                return new clsDriver(DriverIDParam, PersonID, CreatedByUserID, CreatedDate , PersonInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// ASYNCHRONOUSLYLYLY finds a driver by PersonID using the ASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="PersonIDParam">The PersonID to search for.</param>
        /// <returns>A clsDriver object if found; otherwise, null.</returns>
        public static async Task<clsDriver> FindByPersonID(int PersonIDParam) // Sync signature
        {
            // Declare variables for ref parameters
            (bool IsFound , int DriverID, int CreatedByUserID, DateTime CreatedDate) = await clsDriverData.GetDriverInfoByPersonIDAsync(PersonIDParam);

            // Call AAASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                clsPerson PersonInfo = await clsPerson.FindAsync(PersonIDParam);
                // Use private constructor
                return new clsDriver(DriverID, PersonIDParam, CreatedByUserID, CreatedDate , PersonInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AAAASYNCHRONOUSLYLYLY gets a DataTable containing all drivers.
        /// </summary>
        /// <returns>A Task returning a DataTable with driver information.</returns>
        public static async Task<DataTable> GetAllDriversAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsDriverData.GetAllDriversAsync();
        }

        /// <summary>
        /// AAAASYNCHRONOUSLYLYLY saves the current driver object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async suffix
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewDriverAsync())
                    {
                        Mode = enMode.Update;
                        // Ideally, refresh CreatedDate here if it's critical immediately after save
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    return await _UpdateDriverAsync();
            }
            return false;
        }

        /// <summary>
        /// AAAASYNCHRONOUSLYLYLY gets a DataTable of licenses for the current driver instance.
        /// Assumes clsLicense.GetDriverLicensesAsync exists.
        /// </summary>
        /// <returns>Task returning DataTable of licenses.</returns>
        public static async Task<DataTable> GetLicensesAsync(int DriverID) // Async suffix, Instance method
        {
            // ASSUMPTION: clsLicense BLL class has an async static method
            return await clsLicense.GetDriverLicensesAsync(DriverID);
        }

        public static async Task<DataTable> GetInternationalLicenses(int DriverID)
        {
            return await clsInternationalLicense.GetDriverInternationalLicensesAsync(DriverID);
        }


    } // End Class clsDriver
} // End Namespace