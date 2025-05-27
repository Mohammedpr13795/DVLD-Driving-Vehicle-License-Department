using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a License Class entity and handles related business logic.
    /// Find methods are AASYNCHRONOUSLYLY.
    /// Save, GetAll, and Get specific property methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsLicenseClass
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties
        public int LicenseClassID { get; set; }
        public string ClassName { get; set; }
        public string ClassDescription { get; set; }
        public byte MinimumAllowedAge { get; set; }
        public byte DefaultValidityLength { get; set; }
        public float ClassFees { get; set; }

        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsLicenseClass()
        {
            this.LicenseClassID = -1;
            this.ClassName = "";
            this.ClassDescription = "";
            this.MinimumAllowedAge = 18; // Default values as per original
            this.DefaultValidityLength = 10;
            this.ClassFees = 0;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find methods.
        /// </summary>
        private clsLicenseClass(int licenseClassID, string className, string classDescription,
            byte minimumAllowedAge, byte defaultValidityLength, float classFees)
        {
            this.LicenseClassID = licenseClassID;
            this.ClassName = className;
            this.ClassDescription = classDescription;
            this.MinimumAllowedAge = minimumAllowedAge;
            this.DefaultValidityLength = defaultValidityLength;
            this.ClassFees = classFees;
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new license class data via DAL.
        /// </summary>
        private async Task<bool> _AddNewLicenseClassAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.LicenseClassID = await clsLicenseClassData.AddNewLicenseClassAsync(
                this.ClassName, this.ClassDescription, this.MinimumAllowedAge,
                this.DefaultValidityLength, this.ClassFees);
            return (this.LicenseClassID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the license class data via DAL.
        /// </summary>
        private async Task<bool> _UpdateLicenseClassAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLicenseClassData.UpdateLicenseClassAsync(
                this.LicenseClassID, this.ClassName, this.ClassDescription,
                this.MinimumAllowedAge, this.DefaultValidityLength, this.ClassFees);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a license class by ID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="LicenseClassIDParam">The ID to search for.</param>
        /// <returns>A clsLicenseClass object if found; otherwise, null.</returns>
        public static async Task<clsLicenseClass> FindAsync(int LicenseClassIDParam) // Sync signature
        {
            // Declare variables for ref parameters
            string ClassName = ""; string ClassDescription = "";
            byte MinimumAllowedAge = 0; byte DefaultValidityLength = 0; float ClassFees = 0;
            bool IsFound = false;
            (IsFound, ClassName, ClassDescription, MinimumAllowedAge,
            DefaultValidityLength, ClassFees) = await clsLicenseClassData.GetLicenseClassInfoByIDAsync(LicenseClassIDParam);

            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                // Use private constructor
                return new clsLicenseClass(LicenseClassIDParam, ClassName, ClassDescription,
                    MinimumAllowedAge, DefaultValidityLength, ClassFees);
            }
            else
                return null;
        }

        public static async Task<clsLicenseClass> FindAsync(string ClassNameParam) // Sync signature
        {
            // Declare variables for ref parameters
            int LicenseClassID = -1;
            string ClassDescription = "";
            byte MinimumAllowedAge = 0; byte DefaultValidityLength = 0; float ClassFees = 0;
            bool IsFound = false;
            (IsFound, LicenseClassID, ClassDescription, MinimumAllowedAge,
            DefaultValidityLength, ClassFees) = await clsLicenseClassData.GetLicenseClassInfoByClassNameAsync(ClassNameParam);

            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
            {
                // Use private constructor
                return new clsLicenseClass(LicenseClassID, ClassNameParam, ClassDescription,
                    MinimumAllowedAge, DefaultValidityLength, ClassFees);
            }
            else
                return null;
        }


        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a license class by Class Name using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="ClassNameParam">The Class Name to search for.</param>
        /// <returns>A clsLicenseClass object if found; otherwise, null.</returns>
        public static clsLicenseClass Find(string ClassNameParam) // Sync signature
        {
            // Declare variables for ref parameters
            int LicenseClassID = -1; string ClassDescription = "";
            byte MinimumAllowedAge = 0; byte DefaultValidityLength = 0; float ClassFees = 0;

            // Call AASYNCHRONOUSLYLY DAL method
            if (clsLicenseClassData.GetLicenseClassInfoByClassName(
                    ClassNameParam, ref LicenseClassID, ref ClassDescription,
                    ref MinimumAllowedAge, ref DefaultValidityLength, ref ClassFees))
            {
                // Use private constructor
                return new clsLicenseClass(LicenseClassID, ClassNameParam, ClassDescription,
                    MinimumAllowedAge, DefaultValidityLength, ClassFees);
            }
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all license classes.
        /// </summary>
        /// <returns>A Task returning a DataTable with license classes.</returns>
        public static async Task<DataTable> GetAllLicenseClassesAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLicenseClassData.GetAllLicenseClassesAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current license class object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async suffix
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    // Await the ASYNC private add method
                    if (await _AddNewLicenseClassAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    // Await the ASYNC private update method
                    return await _UpdateLicenseClassAsync();
            }
            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the minimum allowed age for a specific license class ID.
        /// </summary>
        /// <param name="licenseClassID">The ID of the license class.</param>
        /// <returns>Task returning the minimum age, or -1 if not found/error.</returns>
        public static async Task<int> GetMinimumAllowedAgeByIDAsync(int licenseClassID) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLicenseClassData.GetMinimumAllowedAgeByIDAsync(licenseClassID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets the class name for a specific license class ID.
        /// </summary>
        /// <param name="licenseClassID">The ID of the license class.</param>
        /// <returns>Task returning the class name, or null if not found/error.</returns>
        public static async Task<string> GetClassNameByIDAsync(int licenseClassID) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsLicenseClassData.GetClassNameByIDAsync(licenseClassID);
        }


        // --- Original AASYNCHRONOUSLYLY Methods (Consider replacing calls with Async versions) ---
        /*
        public static DataTable GetAllLicenseClasses()
        {
            // Sync wrapper (use with caution)
            try { return GetAllLicenseClassesAsync().GetAwaiter().GetResult(); }
            catch { return new DataTable(); }
        }
        public bool Save()
        {
            // Sync wrapper (use with caution)
            try { return SaveAsync().GetAwaiter().GetResult(); }
            catch { return false; }
        }
         public static int GetMinimumAllowedAgeByID(int licenseClassID)
        {
             // Sync wrapper (use with caution)
            try { return GetMinimumAllowedAgeByIDAsync(licenseClassID).GetAwaiter().GetResult(); }
            catch { return -1; }
        }
         public static string GetClassNameByID(int licenseClassID)
        {
            // Sync wrapper (use with caution)
             try { return GetClassNameByIDAsync(licenseClassID).GetAwaiter().GetResult(); }
            catch { return null; }
        }
        */

    } // End Class clsLicenseClass
} // End Namespace