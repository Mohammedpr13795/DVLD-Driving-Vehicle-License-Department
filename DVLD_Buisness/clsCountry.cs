using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Country entity and handles related business logic.
    /// Find methods are AASYNCHRONOUSLYLY.
    /// GetAll and other static Find methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsCountry
    {
        // Properties
        public int ID { get; set; }
        public string CountryName { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public clsCountry()
        {
            this.ID = -1;
            this.CountryName = "";
        }

        /// <summary>
        /// Private constructor for loading existing data. Called by Find methods.
        /// </summary>
        private clsCountry(int id, string countryName)
        {
            this.ID = id;
            this.CountryName = countryName;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a country by ID using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="IDParam">The ID to search for.</param>
        /// <returns>A clsCountry object if found; otherwise, null.</returns>
        public static async Task<clsCountry> FindAsync(int IDParam) // Sync signature
        {
            bool IsFound = false;
            string CountryName = string.Empty; // Variable for ref parameter
            (IsFound, CountryName) = await clsCountryData.GetCountryInfoByIDAsync(IDParam);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
                // Use private constructor
                return new clsCountry(IDParam, CountryName);
            else
                return null;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a country by Name using the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="CountryNameParam">The Name to search for.</param>
        /// <returns>A clsCountry object if found; otherwise, null.</returns>
        public static async Task<clsCountry> Find(string CountryNameParam) // Sync signature
        {
            bool IsFound = false;
            int ID = -1; // Variable for ref parameter
            (IsFound, ID) = await clsCountryData.GetCountryInfoByName(CountryNameParam);
            // Call AASYNCHRONOUSLYLY DAL method
            if (IsFound)
                // Use private constructor
                return new clsCountry(ID, CountryNameParam);
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all countries.
        /// </summary>
        /// <returns>A Task returning a DataTable with countries.</returns>
        public static async Task<DataTable> GetAllCountriesAsync() // Async suffix
        {
            // Call ASYNC DAL method
            return await clsCountryData.GetAllCountriesAsync();
        }


        /// <summary>
        /// AAASYNCHRONOUSLYLYLY finds Country Name by Country ID.
        /// (Formerly FindCountryByNationalID)
        /// </summary>
        /// <param name="CountryID">The Country ID to search for.</param>
        /// <returns>A Task returning the Country Name, or null if not found.</returns>
        public static async Task<string> FindCountryNameByCountryIDAsync(int CountryID) // Renamed and Async suffix
        {
            // Call ASYNC DAL method
            return await clsCountryData.FindCountryNameByCountryIDAsync(CountryID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY finds Country ID by Country Name.
        /// </summary>
        /// <param name="CountryName">The Country Name to search for.</param>
        /// <returns>A Task returning the Country ID, or -1 if not found.</returns>
        public static async Task<int> FindCountryIDByCountryNameAsync(string CountryName) // Async suffix
        {
            // Call ASYNC DAL method
            return await clsCountryData.FindCountryIDByCountryNameAsync(CountryName);
        }


    } // End Class clsCountry
} // End Namespace