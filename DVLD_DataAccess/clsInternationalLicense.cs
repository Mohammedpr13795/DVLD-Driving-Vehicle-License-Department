using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for InternationalLicense entities using Stored Procedures.
    /// GetInternationalLicenseInfoByID is SYNCHRONOUS to support ref parameters from BLL.
    /// Other methods are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsInternationalLicenseData
    {
        // --- SYNCHRONOUS Method (To support BLL.Find with ref) ---

        /// <summary>
        /// ASYNCHRONOUS retrieves International License information by ID. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound,int ApplicationID, int DriverID, int IssuedUsingLocalLicenseID,
            DateTime IssueDate, DateTime ExpirationDate, bool IsActive, int CreatedByUserID)>
            GetInternationalLicenseInfoByIDAsync(int InternationalLicenseID)
        {
            bool isFound = false;
            // Initialize refs
            int applicationID = -1, driverID = -1, issuedUsingLocalLicenseID = -1, createdByUserID = -1;
            DateTime issueDate = DateTime.MinValue, expirationDate = DateTime.MinValue;
            bool isActive = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetInternationalLicenseInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@InternationalLicenseID", InternationalLicenseID);

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
                                issuedUsingLocalLicenseID = (int)reader["IssuedUsingLocalLicenseID"];
                                issueDate = (DateTime)reader["IssueDate"];
                                expirationDate = (DateTime)reader["ExpirationDate"];
                                isActive = (bool)reader["IsActive"];
                                createdByUserID = (int)reader["CreatedByUserID"]; // Corrected from original code
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetInternationalLicenseInfoByID (IntLicenseID: {InternationalLicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , ApplicationID:applicationID , DriverID:driverID , IssuedUsingLocalLicenseID:issuedUsingLocalLicenseID,
                    IssueDate:issueDate , ExpirationDate:expirationDate , IsActive:isActive , CreatedByUserID:createdByUserID);
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// AASYNCHRONOUS retrieves all international licenses using SP_GetAllInternationalLicenses.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllInternationalLicensesAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllInternationalLicenses", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllInternationalLicensesAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllInternationalLicensesAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// AASYNCHRONOUS retrieves international licenses for a specific driver using SP_GetDriverInternationalLicenses.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetDriverInternationalLicensesAsync(int DriverID) // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetDriverInternationalLicenses", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetDriverInternationalLicensesAsync (DriverID: {DriverID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetDriverInternationalLicensesAsync (DriverID: {DriverID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }


        /// <summary>
        /// AASYNCHRONOUS adds a new international license using SP_AddNewInternationalLicense.
        /// The SP handles deactivating previous active licenses for the driver.
        /// </summary>
        /// <returns>A Task returning the new InternationalLicenseID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewInternationalLicenseAsync(int ApplicationID, // Async suffix
             int DriverID, int IssuedUsingLocalLicenseID,
             DateTime IssueDate, DateTime ExpirationDate, bool IsActive, int CreatedByUserID)
        {
            int internationalLicenseID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewInternationalLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@DriverID", DriverID);
                    command.Parameters.AddWithValue("@IssuedUsingLocalLicenseID", IssuedUsingLocalLicenseID);
                    command.Parameters.AddWithValue("@IssueDate", IssueDate);
                    command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate);
                    command.Parameters.AddWithValue("@IsActive", IsActive);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewInternationalLicenseID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync(); // SP handles transaction

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            internationalLicenseID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in AddNewInternationalLicenseAsync (AppID: {ApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in AddNewInternationalLicenseAsync (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return internationalLicenseID;
        }

        /// <summary>
        /// AASYNCHRONOUS updates an existing international license using SP_UpdateInternationalLicense.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateInternationalLicenseAsync( // Async suffix
              int InternationalLicenseID, int ApplicationID,
             int DriverID, int IssuedUsingLocalLicenseID,
             DateTime IssueDate, DateTime ExpirationDate, bool IsActive, int CreatedByUserID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateInternationalLicense", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@InternationalLicenseID", InternationalLicenseID);
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@DriverID", DriverID);
                    command.Parameters.AddWithValue("@IssuedUsingLocalLicenseID", IssuedUsingLocalLicenseID);
                    command.Parameters.AddWithValue("@IssueDate", IssueDate);
                    command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate);
                    command.Parameters.AddWithValue("@IsActive", IsActive);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in UpdateInternationalLicenseAsync (IntLicenseID: {InternationalLicenseID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in UpdateInternationalLicenseAsync (IntLicenseID: {InternationalLicenseID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// AASYNCHRONOUS gets the active InternationalLicenseID for a specific driver using SP_GetActiveInternationalLicenseIDByDriverID.
        /// </summary>
        /// <returns>A Task returning the active InternationalLicenseID, or -1 if not found or error occurs.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> GetActiveInternationalLicenseIDByDriverIDAsync(int DriverID) // Async suffix
        {
            int InternationalLicenseID = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetActiveInternationalLicenseIDByDriverID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DriverID", DriverID);

                    try
                    {
                        await connection.OpenAsync();
                        object result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        {
                            InternationalLicenseID = insertedID;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetActiveInternationalLicenseIDByDriverIDAsync (DriverID: {DriverID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); InternationalLicenseID = -1; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetActiveInternationalLicenseIDByDriverIDAsync (DriverID: {DriverID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); InternationalLicenseID = -1; throw; }
                }
            }
            return InternationalLicenseID;
        }

    } // End Class clsInternationalLicenseData
} // End Namespace