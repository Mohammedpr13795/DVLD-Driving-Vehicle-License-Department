using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents an Application Type entity and handles related business logic.
    /// Find method is AASYNCHRONOUSLYLY.
    /// Save and GetAll methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsApplicationType
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties
        public int ID { get; set; }
        public string Title { get; set; }
        public float Fees { get; set; }

        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsApplicationType()
        {
            this.ID = -1;
            this.Title = "";
            this.Fees = 0;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find method.
        /// </summary>
        private clsApplicationType(int id, string title, float fees)
        {
            this.ID = id;
            this.Title = title;
            this.Fees = fees;
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new application type data via DAL.
        /// </summary>
        private async Task<bool> _AddNewApplicationTypeAsync() // Async suffix
        {
            // Call the ASYNC DAL method
            this.ID = await clsApplicationTypeData.AddNewApplicationTypeAsync(this.Title, this.Fees);
            return (this.ID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the application type data via DAL.
        /// </summary>
        private async Task<bool> _UpdateApplicationTypeAsync() // Async suffix
        {
            // Call the ASYNC DAL method
            return await clsApplicationTypeData.UpdateApplicationTypeAsync(this.ID, this.Title, this.Fees);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds an application type by ID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="IDParam">The ID to search for.</param>
        /// <returns>A clsApplicationType object if found; otherwise, null.</returns>
        public static async Task<clsApplicationType> FindAsync(int IDParam) // Sync signature
        {
            // Declare variables for Tupple parameters
            bool IsFound = false;
            string Title = "";
            float Fees = 0;
            (IsFound, Title, Fees) = await clsApplicationTypeData.GetApplicationTypeInfoByIDAsync(IDParam);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
                // Use private constructor
                return new clsApplicationType(IDParam, Title, Fees);
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all application types.
        /// </summary>
        /// <returns>A Task returning a DataTable with application types.</returns>
        public static async Task<DataTable> GetAllApplicationTypesAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsApplicationTypeData.GetAllApplicationTypesAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current application type object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async suffix
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    // Await the ASYNC private add method
                    if (await _AddNewApplicationTypeAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    // Await the ASYNC private update method
                    return await _UpdateApplicationTypeAsync();
            }
            return false;
        }

        // --- Original AASYNCHRONOUSLYLY Methods (Consider replacing calls with Async versions) ---
        /*
        public static DataTable GetAllApplicationTypes()
        {
            // AASYNCHRONOUSLYLY wrapper (use with caution in UI thread)
            try { return GetAllApplicationTypesAsync().GetAwaiter().GetResult(); }
            catch { return new DataTable(); } // Or handle error
        }

        public bool Save()
        {
             // AASYNCHRONOUSLYLY wrapper (use with caution in UI thread)
            try { return SaveAsync().GetAwaiter().GetResult(); }
            catch { return false; } // Or handle error
        }
        */

    } // End Class clsApplicationType
} // End Namespace