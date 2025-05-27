using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for License entities using Stored Procedures.
    /// GetLicenseInfoByID is ASYNCHRONOUS to support ref parameters from BLL.
    /// Other methods are AASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsLicenseData
    {
        // --- ASYNCHRONOUS Method (To support BLL.Find with ref) ---

        /// <summary>
        /// ASYNCHRONOUS retrieves License information by LicenseID using SP_GetLicenseInfoByID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int ApplicationID, int DriverID, int LicenseClass,
            DateTime IssueDate, DateTime ExpirationDate, string Notes,
            float PaidFees, bool IsActive, byte IssueReason,  int CreatedByUserID)
            > GetLicenseInfoByIDAsync(int LicenseID)
        {
            bool isFound = false;
            int applicationID = -1,driverID = -1,licenseClass = -1,createdByUserID = -1;
            DateTime issueDate = DateTime.MinValue, expirationDate = DateTime.MinValue;
            string notes = string.Empty;
            float PaidFees = -1; bool isActive = false; byte issueReason = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLicenseInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                applicationID = (int)reader["ApplicationID"];
                                driverID = (int)reader["DriverID"];
                                licenseClass = (int)reader["LicenseClass"];
                                issueDate = (DateTime)reader["IssueDate"];
                                expirationDate = (DateTime)reader["ExpirationDate"];
                                notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null; // Handle null
                                PaidFees = Convert.ToSingle(reader["PaidFees"]);
                                isActive = (bool)reader["IsActive"];
                                issueReason = (byte)reader["IssueReason"];
                                createdByUserID = (int)reader["CreatedByUserID"]; // Corrected from DriverID in original code
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLicenseInfoByID (LicenseID: {LicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , ApplicationID:applicationID , DriverID:driverID , LicenseClass:licenseClass ,
                    IssueDate:issueDate , ExpirationDate:expirationDate , Notes:notes , PaidFees:PaidFees , IsActive :isActive,
                    IssueReason:issueReason , CreatedByUserID:createdByUserID);
        }


        // --- AASYNCHRONOUS Methods ---

        /// <summary>
        /// AAASYNCHRONOUS retrieves all licenses using SP_GetAllLicenses.
        /// </summary>
        /// <returns>A Task returning a DataTable with all licenses.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllLicensesAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllLicenses", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllLicensesAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllLicensesAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// AAASYNCHRONOUS retrieves licenses for a specific driver using SP_GetDriverLicenses.
        /// </summary>
        /// <param name="DriverID">The ID of the driver.</param>
        /// <returns>A Task returning a DataTable with the driver's licenses.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetDriverLicensesAsync(int DriverID) // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetDriverLicenses", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DriverID", DriverID);
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows) { dt.Load(reader); }
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetDriverLicensesAsync (DriverID: {DriverID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetDriverLicensesAsync (DriverID: {DriverID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// AAASYNCHRONOUS adds a new license using SP_AddNewLicense.
        /// </summary>
        /// <returns>A Task returning the new LicenseID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewLicenseAsync(int ApplicationID, int DriverID, int LicenseClass,
             DateTime IssueDate, DateTime ExpirationDate, string Notes,
             float PaidFees, bool IsActive, byte IssueReason, int CreatedByUserID) // Async suffix
        {
            int licenseID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@DriverID", DriverID);
                    command.Parameters.AddWithValue("@LicenseClass", LicenseClass);
                    command.Parameters.AddWithValue("@IssueDate", IssueDate);
                    command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate);
                    command.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(Notes) ? (object)DBNull.Value : Notes); // Handle null/empty
                    command.Parameters.AddWithValue("@PaidFees", PaidFees);
                    command.Parameters.AddWithValue("@IsActive", IsActive);
                    command.Parameters.AddWithValue("@IssueReason", IssueReason);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewLicenseID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            licenseID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in AddNewLicenseAsync (AppID: {ApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in AddNewLicenseAsync (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return licenseID;
        }

        /// <summary>
        /// AAASYNCHRONOUS updates an existing license using SP_UpdateLicense.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateLicenseAsync(int LicenseID, int ApplicationID, int DriverID, int LicenseClass,
             DateTime IssueDate, DateTime ExpirationDate, string Notes,
             float PaidFees, bool IsActive, byte IssueReason, int CreatedByUserID) // Async suffix
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@DriverID", DriverID);
                    command.Parameters.AddWithValue("@LicenseClass", LicenseClass);
                    command.Parameters.AddWithValue("@IssueDate", IssueDate);
                    command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate);
                    command.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(Notes) ? (object)DBNull.Value : Notes);
                    command.Parameters.AddWithValue("@PaidFees", PaidFees);
                    command.Parameters.AddWithValue("@IsActive", IsActive);
                    command.Parameters.AddWithValue("@IssueReason", IssueReason);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in UpdateLicenseAsync (LicenseID: {LicenseID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in UpdateLicenseAsync (LicenseID: {LicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// AAASYNCHRONOUS gets the active LicenseID for a specific person and license class using SP_GetActiveLicenseIDByPersonID.
        /// </summary>
        /// <returns>A Task returning the active LicenseID, or -1 if not found or error occurs.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> GetActiveLicenseIDByPersonIDAsync(int PersonID, int LicenseClassID) // Async suffix
        {
            int licenseID = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetActiveLicenseIDByPersonID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", PersonID);
                    command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID); // Match SP parameter name

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            int.TryParse(result.ToString(), out licenseID);
                            if (licenseID == 0) licenseID = -1; // Handle non-identity PKs if needed
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetActiveLicenseIDByPersonIDAsync (PersonID: {PersonID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); licenseID = -1; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetActiveLicenseIDByPersonIDAsync (PersonID: {PersonID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); licenseID = -1; throw; }
                }
            }
            return licenseID;
        }

        /// <summary>
        /// AAASYNCHRONOUS deactivates a license by setting IsActive to 0 using SP_DeactivateLicense.
        /// </summary>
        /// <returns>A Task returning true if deactivation was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> DeactivateLicenseAsync(int LicenseID) // Async suffix
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_DeactivateLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);
                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in DeactivateLicenseAsync (LicenseID: {LicenseID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in DeactivateLicenseAsync (LicenseID: {LicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }

    } // End Class clsLicenseData
} // End Namespace