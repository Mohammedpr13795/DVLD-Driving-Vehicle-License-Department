using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for Country entities using Stored Procedures.
    /// Get methods using ref parameters are ASYNCHRONOUS.
    /// Other methods (GetAll, Find...) are AASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsCountryData
    {
        // Note: enGendor enum seems misplaced here, belongs likely in Person related classes. Removed.

        // --- ASYNCHRONOUS Methods (To support BLL.Find with ref) ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves Country Name by ID. Uses ref parameter. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , string CountryName)> GetCountryInfoByIDAsync(int ID)
        {
            bool isFound = false;
            string countryName = null; // Initialize ref

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetCountryInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CountryID", ID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        object result = await command.ExecuteScalarAsync(); // Use ExecuteScalar for single value

                        if (result != null && result != DBNull.Value)
                        {
                            isFound = true;
                            countryName = (string)result;
                        }
                        // else: not found, isFound remains false
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetCountryInfoByID (ID: {ID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , CountryName:countryName);
        }

        /// <summary>
        /// ASYNCHRONOUSLY retrieves Country ID by Name. Uses ref parameter. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int ID)> GetCountryInfoByName(string CountryName)
        {
            bool isFound = false;
            int id = -1; // Initialize ref

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetCountryInfoByName", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CountryName", CountryName);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        object result = await command.ExecuteScalarAsync(); // Use ExecuteScalar

                        if (result != null && result != DBNull.Value)
                        {
                            isFound = true;
                            id = (int)result;
                        }
                        // else: not found, isFound remains false
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetCountryInfoByName (Name: {CountryName}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , ID:id);
        }


        // --- AASYNCHRONOUS Methods ---

        /// <summary>
        /// AASYNCHRONOUSLY retrieves all countries using SP_GetAllCountries.
        /// </summary>
        /// <returns>A Task returning a DataTable with all countries.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllCountriesAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllCountries", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows) { dt.Load(reader); }
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllCountriesAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllCountriesAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// AASYNCHRONOUSLY finds Country Name by Country ID using SP_FindCountryNameByCountryID.
        /// (Formerly FindCountryByNationalID)
        /// </summary>
        /// <param name="CountryID">The Country ID to search for.</param>
        /// <returns>A Task returning the Country Name, or null if not found.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<string> FindCountryNameByCountryIDAsync(int CountryID) // Renamed and Async suffix
        {
            string countryName = null;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_FindCountryNameByCountryID", connection)) // Use renamed SP
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CountryID", CountryID); // Use correct parameter name

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync(); // Use Async
                        if (result != null && result != DBNull.Value)
                        {
                            countryName = (string)result;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in FindCountryNameByCountryIDAsync (ID: {CountryID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); countryName = null; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in FindCountryNameByCountryIDAsync (ID: {CountryID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); countryName = null; throw; }
                }
            }
            return countryName;
        }

        /// <summary>
        /// AASYNCHRONOUSLY finds Country ID by Country Name using SP_FindCountryIDByCountryName.
        /// </summary>
        /// <param name="CountryName">The Country Name to search for.</param>
        /// <returns>A Task returning the Country ID, or -1 if not found.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> FindCountryIDByCountryNameAsync(string CountryName) // Async suffix
        {
            int countryID = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_FindCountryIDByCountryName", connection)) // Use correct SP
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CountryName", CountryName);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync(); // Use Async
                        if (result != null && result != DBNull.Value)
                        {
                            int.TryParse(result.ToString(), out countryID);
                            if (countryID == 0) countryID = -1; // Handle non-identity PK if necessary
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in FindCountryIDByCountryNameAsync (Name: {CountryName}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); countryID = -1; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in FindCountryIDByCountryNameAsync (Name: {CountryName}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); countryID = -1; throw; }
                }
            }
            return countryID;
        }

    } // End Class clsCountryData
} // End Namespace