using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for LocalDrivingLicenseApplication entities using Stored Procedures.
    /// Get methods using ref parameters are SYNCHRONOUS.
    /// Other methods are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsLocalDrivingLicenseApplicationData
    {
        // --- SYNCHRONOUS Methods (To support BLL.Find with ref) ---

        /// <summary>
        /// SYNCHRONOUSLY retrieves Local Driving License Application info by ID. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int ApplicationID, int LicenseClassID)> 
            GetLocalDrivingLicenseApplicationInfoByIDAsync(int LocalDrivingLicenseApplicationID)
        {
            bool isFound = false;
            int applicationID = -1, licenseClassID = -1; // Initialize refs

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLocalDrivingLicenseApplicationInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                applicationID = (int)reader["ApplicationID"];
                                licenseClassID = (int)reader["LicenseClassID"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLocalDrivingLicenseApplicationInfoByID (LDLA_ID: {LocalDrivingLicenseApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound, ApplicationID:applicationID , LicenseClassID:licenseClassID);
        }

        /// <summary>
        /// SYNCHRONOUSLY retrieves Local Driving License Application info by the base ApplicationID. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int LocalDrivingLicenseApplicationID, int LicenseClassID)> 
            GetLocalDrivingLicenseApplicationInfoByApplicationIDAsync(int ApplicationID)
        {
            bool isFound = false;
            int localDrivingLicenseApplicationID = -1,licenseClassID = -1; // Initialize refs

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLocalDrivingLicenseApplicationInfoByApplicationID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                localDrivingLicenseApplicationID = (int)reader["LocalDrivingLicenseApplicationID"];
                                licenseClassID = (int)reader["LicenseClassID"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLocalDrivingLicenseApplicationInfoByApplicationID (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , LocalDrivingLicenseApplicationID:localDrivingLicenseApplicationID , LicenseClassID:licenseClassID);
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves all local driving license applications using SP_GetAllLocalDrivingLicenseApplications (which uses a view).
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllLocalDrivingLicenseApplicationsAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                // Using SP that selects from the view
                using (SqlCommand command = new SqlCommand("SP_GetAllLocalDrivingLicenseApplications", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                dt.Load(reader); // Sync load
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetAllLocalDrivingLicenseApplicationsAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetAllLocalDrivingLicenseApplicationsAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY adds a new local driving license application record using SP_AddNewLocalDrivingLicenseApplication.
        /// </summary>
        /// <returns>A Task returning the new LocalDrivingLicenseApplicationID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewLocalDrivingLicenseApplicationAsync( // Async suffix
            int ApplicationID, int LicenseClassID)
        {
            int localDrivingLicenseApplicationID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewLocalDrivingLicenseApplication", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewLocalDrivingLicenseApplicationID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            localDrivingLicenseApplicationID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in AddNewLocalDrivingLicenseApplicationAsync (AppID: {ApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in AddNewLocalDrivingLicenseApplicationAsync (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return localDrivingLicenseApplicationID;
        }


        /// <summary>
        /// ASYNCHRONOUSLY updates an existing local driving license application record using SP_UpdateLocalDrivingLicenseApplication.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateLocalDrivingLicenseApplicationAsync( // Async suffix
            int LocalDrivingLicenseApplicationID, int ApplicationID, int LicenseClassID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateLocalDrivingLicenseApplication", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in UpdateLocalDrivingLicenseApplicationAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UpdateLocalDrivingLicenseApplicationAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }


        /// <summary>
        /// ASYNCHRONOUSLY deletes a local driving license application by ID using SP_DeleteLocalDrivingLicenseApplication.
        /// </summary>
        /// <returns>A Task returning true if deletion was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> DeleteLocalDrivingLicenseApplicationAsync(int LocalDrivingLicenseApplicationID) // Async suffix
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_DeleteLocalDrivingLicenseApplication", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in DeleteLocalDrivingLicenseApplicationAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in DeleteLocalDrivingLicenseApplicationAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if the latest test of a specific type was passed for the application using SP_DoesPassTestType.
        /// </summary>
        /// <returns>A Task returning true if the latest test was passed, false otherwise (including not found or error).</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> DoesPassTestTypeAsync(int LocalDrivingLicenseApplicationID, int TestTypeID) // Async suffix
        {
            bool result = false; // Default to not passed
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_DoesPassTestType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

                    try
                    {
                        await connection.OpenAsync();
                        object scalarResult = await command.ExecuteScalarAsync();

                        // Check if a result was returned and if it represents 'true' (passed)
                        if (scalarResult != null && scalarResult != DBNull.Value)
                        {
                            bool.TryParse(scalarResult.ToString(), out result);
                        }
                        // If scalarResult is null (no test found), result remains false.
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in DoesPassTestTypeAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in DoesPassTestTypeAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if the applicant attended (took) any test of a specific type for the application using SP_DoesAttendTestType.
        /// </summary>
        /// <returns>A Task returning true if attended, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> DoesAttendTestTypeAsync(int LocalDrivingLicenseApplicationID, int TestTypeID) // Async suffix
        {
            bool IsFound = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_DoesAttendTestType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);
                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            IsFound = true;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in DoesAttendTestTypeAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in DoesAttendTestTypeAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return IsFound;
        }

        /// <summary>
        /// ASYNCHRONOUSLY gets the total number of trials (tests taken) for a specific test type and application using SP_GetTotalTrialsPerTest.
        /// </summary>
        /// <returns>A Task returning the total number of trials (byte), or 0 on error.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<byte> TotalTrialsPerTestAsync(int LocalDrivingLicenseApplicationID, int TestTypeID) // Async suffix
        {
            byte totalTrials = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetTotalTrialsPerTest", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);
                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            // SP returns INT, convert carefully to byte
                            int trialsInt = Convert.ToInt32(result);
                            totalTrials = (trialsInt > byte.MaxValue) ? byte.MaxValue : Convert.ToByte(trialsInt);
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in TotalTrialsPerTestAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        totalTrials = 0; // Default on error
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in TotalTrialsPerTestAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        totalTrials = 0; // Default on error
                        throw; // Re-throw
                    }
                }
            }
            return totalTrials;
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if there is an active (not locked) scheduled test of a specific type for the application using SP_IsThereAnActiveScheduledTest.
        /// </summary>
        /// <returns>A Task returning true if an active test appointment exists, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> IsThereAnActiveScheduledTestAsync(int LocalDrivingLicenseApplicationID, int TestTypeID) // Async suffix
        {
            bool result = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_IsThereAnActiveScheduledTest", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);
                    try
                    {
                        await connection.OpenAsync();
                        object scalarResult = await command.ExecuteScalarAsync();
                        if (scalarResult != null && scalarResult != DBNull.Value)
                        {
                            bool.TryParse(scalarResult.ToString(), out result); // SP returns BIT
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in IsThereAnActiveScheduledTestAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in IsThereAnActiveScheduledTestAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return result;
        }

    } // End Class clsLocalDrivingLicenseApplicationData
} // End Namespace