using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for DetainedLicense entities using Stored Procedures.
    /// Get methods using ref parameters are SYNCHRONOUS.
    /// Other methods are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsDetainedLicenseData
    {
        // --- SYNCHRONOUS Methods (To support BLL.Find with ref) ---

        /// <summary>
        /// SYNCHRONOUSLY retrieves Detained License information by DetainID. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound ,int LicenseID, DateTime DetainDate,float FineFees, int CreatedByUserID,
            bool IsReleased, DateTime ReleaseDate,int ReleasedByUserID, int ReleaseApplicationID)> 
            GetDetainedLicenseInfoByIDAsync(int DetainID)
        {
            bool isFound = false;
            // Initialize refs
            int licenseID = -1; float fineFees = 0; int createdByUserID = -1;int releasedByUserID = -1; 
            int releaseApplicationID = -1; DateTime detainDate = DateTime.MinValue;DateTime releaseDate = DateTime.MinValue;
            bool isReleased = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetDetainedLicenseInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DetainID", DetainID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                licenseID = (int)reader["LicenseID"];
                                detainDate = (DateTime)reader["DetainDate"];
                                fineFees = Convert.ToSingle(reader["FineFees"]);
                                createdByUserID = (int)reader["CreatedByUserID"];
                                isReleased = (bool)reader["IsReleased"];
                                releaseDate = reader["ReleaseDate"] != DBNull.Value ? (DateTime)reader["ReleaseDate"] : DateTime.MinValue; // Use MinValue for null date
                                releasedByUserID = reader["ReleasedByUserID"] != DBNull.Value ? (int)reader["ReleasedByUserID"] : -1;
                                releaseApplicationID = reader["ReleaseApplicationID"] != DBNull.Value ? (int)reader["ReleaseApplicationID"] : -1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetDetainedLicenseInfoByID (DetainID: {DetainID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound: isFound, LicenseID: licenseID, DetainDate: detainDate, FineFees: fineFees, CreatedByUserID: createdByUserID,
            IsReleased: isReleased, ReleaseDate: releaseDate, ReleasedByUserID: releasedByUserID, ReleaseApplicationID: releaseApplicationID);
        }


        /// <summary>
        /// SYNCHRONOUSLY retrieves the latest Detained License information by LicenseID. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static bool GetDetainedLicenseInfoByLicenseID(int LicenseID,
         ref int DetainID, ref DateTime DetainDate,
         ref float FineFees, ref int CreatedByUserID,
         ref bool IsReleased, ref DateTime ReleaseDate,
         ref int ReleasedByUserID, ref int ReleaseApplicationID)
        {
            bool isFound = false;
            // Initialize refs
            DetainID = -1; FineFees = 0; CreatedByUserID = -1;
            ReleasedByUserID = -1; ReleaseApplicationID = -1; ReleaseDate = DateTime.MinValue;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetDetainedLicenseInfoByLicenseID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);

                    try
                    {
                        connection.Open(); // Sync
                        using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow)) // Sync
                        {
                            if (reader.Read()) // Sync
                            {
                                isFound = true;
                                DetainID = (int)reader["DetainID"];
                                DetainDate = (DateTime)reader["DetainDate"];
                                FineFees = Convert.ToSingle(reader["FineFees"]);
                                CreatedByUserID = (int)reader["CreatedByUserID"];
                                IsReleased = (bool)reader["IsReleased"];
                                ReleaseDate = reader["ReleaseDate"] != DBNull.Value ? (DateTime)reader["ReleaseDate"] : DateTime.MinValue;
                                ReleasedByUserID = reader["ReleasedByUserID"] != DBNull.Value ? (int)reader["ReleasedByUserID"] : -1;
                                ReleaseApplicationID = reader["ReleaseApplicationID"] != DBNull.Value ? (int)reader["ReleaseApplicationID"] : -1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetDetainedLicenseInfoByLicenseID (LicenseID: {LicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return isFound;
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves all detained licenses using SP_GetAllDetainedLicenses (uses view).
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllDetainedLicensesAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllDetainedLicenses", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllDetainedLicensesAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllDetainedLicensesAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY adds a new detained license record using SP_AddNewDetainedLicense.
        /// Important: Deactivating the original license should be handled separately (e.g., in BLL).
        /// </summary>
        /// <returns>A Task returning the new DetainID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewDetainedLicenseAsync( // Async suffix
            int LicenseID, DateTime DetainDate,
            float FineFees, int CreatedByUserID)
        {
            int detainID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewDetainedLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);
                    command.Parameters.AddWithValue("@DetainDate", DetainDate);
                    command.Parameters.AddWithValue("@FineFees", FineFees);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewDetainID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            detainID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in AddNewDetainedLicenseAsync (LicenseID: {LicenseID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in AddNewDetainedLicenseAsync (LicenseID: {LicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return detainID;
        }

        /// <summary>
        /// ASYNCHRONOUSLY updates basic info of a detained license record using SP_UpdateDetainedLicense.
        /// Note: This method is less common; release updates are handled by ReleaseDetainedLicenseAsync.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateDetainedLicenseAsync(int DetainID, // Async suffix
            int LicenseID, DateTime DetainDate,
            float FineFees, int CreatedByUserID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateDetainedLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters (Corrected @DetainedLicenseID to @DetainID)
                    command.Parameters.AddWithValue("@DetainID", DetainID); // Use correct PK name
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);
                    command.Parameters.AddWithValue("@DetainDate", DetainDate);
                    command.Parameters.AddWithValue("@FineFees", FineFees);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in UpdateDetainedLicenseAsync (DetainID: {DetainID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in UpdateDetainedLicenseAsync (DetainID: {DetainID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }


        /// <summary>
        /// ASYNCHRONOUSLY releases a detained license using SP_ReleaseDetainedLicense.
        /// The SP handles updating DetainedLicenses and reactivating the License record.
        /// </summary>
        /// <returns>A Task returning true if release was successful (SP executed without error), false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> ReleaseDetainedLicenseAsync(int DetainID, // Async suffix
                 int ReleasedByUserID, int ReleaseApplicationID)
        {
            int rowsAffected = 0; // Not directly relevant as SP does multiple updates
            bool success = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_ReleaseDetainedLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@DetainID", DetainID);
                    command.Parameters.AddWithValue("@ReleasedByUserID", ReleasedByUserID);
                    command.Parameters.AddWithValue("@ReleaseApplicationID", ReleaseApplicationID);
                    // ReleaseDate is handled by GETDATE() in the SP

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync(); // Execute SP which handles transaction
                        success = true; // Assume success if no exception
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in ReleaseDetainedLicenseAsync (DetainID: {DetainID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); success = false; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in ReleaseDetainedLicenseAsync (DetainID: {DetainID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); success = false; throw; }
                }
            }
            return success;
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if a license is currently detained (IsReleased = 0) using SP_IsLicenseDetained.
        /// </summary>
        /// <returns>A Task returning true if the license is detained, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> IsLicenseDetainedAsync(int LicenseID) // Async suffix
        {
            bool isDetained = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_IsLicenseDetained", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            isDetained = Convert.ToBoolean(result); // SP returns BIT
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in IsLicenseDetainedAsync (LicenseID: {LicenseID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); isDetained = false; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in IsLicenseDetainedAsync (LicenseID: {LicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); isDetained = false; throw; }
                }
            }
            return isDetained;
        }

    } // End Class clsDetainedLicenseData
} // End Namespace