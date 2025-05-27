using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Test Type entity and handles related business logic.
    /// Find method is AASYNCHRONOUSLYLY. Save and GetAll methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsTestType
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enum enTestType { VisionTest = 1, WrittenTest = 2, StreetTest = 3 }; // Keep the enum for logical representation

        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties - Corrected ID to be int
        public int TestTypeID { get; set; } // The actual ID (Primary Key) from the database
        public string Title { get; set; }
        public string Description { get; set; }
        public float Fees { get; set; }

        // Optional: Read-only property to get the enum value based on ID
        public enTestType Type
        {
            get { return (enTestType)this.TestTypeID; }
        }


        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsTestType()
        {
            this.TestTypeID = -1; // Initialize ID as -1 for new records
            this.Title = "";
            this.Description = "";
            this.Fees = 0;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// Called by Find method.
        /// </summary>
        private clsTestType(int testTypeID, string title, string description, float fees)
        {
            this.TestTypeID = testTypeID; // Use the int ID
            this.Title = title;
            this.Description = description;
            this.Fees = fees;
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new test type data via DAL.
        /// </summary>
        private async Task<bool> _AddNewTestTypeAsync() // Async suffix
        {
            // Call ASYNC DAL method
            this.TestTypeID = await clsTestTypeData.AddNewTestTypeAsync(this.Title, this.Description, this.Fees);
            return (this.TestTypeID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the test type data via DAL.
        /// </summary>
        private async Task<bool> _UpdateTestTypeAsync() // Async suffix
        {
             // Call ASYNC DAL method
            return await clsTestTypeData.UpdateTestTypeAsync(this.TestTypeID, this.Title, this.Description, this.Fees);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a test type by its integer ID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="TestTypeIDParam">The integer ID to search for.</param>
        /// <returns>A clsTestType object if found; otherwise, null.</returns>
        public static async Task<clsTestType> FindAsync(int TestTypeIDParam) // Parameter is int
        {
            // Declare variables for ref parameters
            bool IsFound = false; string Title = "", Description = ""; float Fees = 0;
            (IsFound, Title, Description, Fees) = await clsTestTypeData.GetTestTypeInfoByIDAsync(TestTypeIDParam);

            // Call AASYNCHRONOUSLYLY DAL method with int ID
            if (IsFound)
                // Use private constructor with int ID
                return new clsTestType(TestTypeIDParam, Title, Description, Fees);
            else
                return null;
        }

         /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a test type by its enum value.
        /// This is a convenience method that calls Find(int).
        /// </summary>
        /// <param name="TestTypeEnum">The enum value to search for.</param>
        /// <returns>A clsTestType object if found; otherwise, null.</returns>
        public static Task<clsTestType> Find(enTestType TestTypeEnum) // Overload for convenience
        {
            // Cast the enum to int and call the other Find method
            return FindAsync((int)TestTypeEnum);
        }


        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all test types.
        /// </summary>
        /// <returns>A Task returning a DataTable with test types.</returns>
        public static async Task<DataTable> GetAllTestTypesAsync() // Async suffix
        {
             // Call ASYNC DAL method
            return await clsTestTypeData.GetAllTestTypesAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current test type object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async suffix
        {
            switch (Mode)
            {
                case enMode.AddNew:
                     // Await the ASYNC private add method
                    if (await _AddNewTestTypeAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    // Await the ASYNC private update method
                    return await _UpdateTestTypeAsync();
            }
            return false;
        }

        // --- Original AASYNCHRONOUSLYLY Methods (Consider replacing calls with Async versions) ---
        /*
        public static DataTable GetAllTestTypes() => GetAllTestTypesAsync().GetAwaiter().GetResult();
        public bool Save() => SaveAsync().GetAwaiter().GetResult();
        */

    } // End Class clsTestType
} // End Namespace