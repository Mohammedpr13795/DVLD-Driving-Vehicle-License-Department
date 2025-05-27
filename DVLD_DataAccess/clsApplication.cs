using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for Application entities using Stored Procedures.
    /// GetApplicationInfoByID is SYNCHRONOUS to support ref parameters from BLL.
    /// Other methods are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsApplicationData
    {
        // --- SYNCHRONOUS Method (To support BLL.FindBaseApplication) ---

        /// <summary>
        /// SYNCHRONOUSLY retrieves Application information by ApplicationID using SP_GetApplicationInfoByID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if the application is found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs general exceptions. Returns false.</exception>

        public static async Task<(bool IsFound , int ApplicantPersonID , DateTime ApplicationDate, int ApplicationTypeID,
                                  byte ApplicationStatus, DateTime LastStatusDate,float PaidFees, int CreatedByUserID)> 
            GetApplicationInfoByIDAsync(int ApplicationID)
        {
            bool isFound = false;
            int applicantPersonID = -1, applicationTypeID = -1, createdByUserID = -1;
            DateTime applicationDate = DateTime.MinValue, lastStatusDate = DateTime.MinValue;
            float paidFees = -1;
            byte applicationStatus= 0;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetApplicationInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);

                    try
                    {
                        await connection.OpenAsync(); // Sync Open
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync Execute
                        {
                            if (await reader.ReadAsync()) // Sync Read
                            {
                                // Read data, handling DBNull is less likely for these fields but good practice
                                isFound = true;
                                // Read data, handling DBNull is less likely for these fields but good practice
                                applicantPersonID = (int)reader["ApplicantPersonID"];
                                applicationDate = (DateTime)reader["ApplicationDate"];
                                applicationTypeID = (int)reader["ApplicationTypeID"];
                                applicationStatus = (byte)reader["ApplicationStatus"];
                                lastStatusDate = (DateTime)reader["LastStatusDate"];
                                paidFees = Convert.ToSingle(reader["PaidFees"]); // float maps to real/float in SQL
                                createdByUserID = (int)reader["CreatedByUserID"];

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetApplicationInfoByID (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                        // Do not re-throw, let BLL handle the 'false' return
                    }
                }

            }

            /*
                                    Task<(bool IsFound , DateTime ApplicationDate, int ApplicationTypeID,
                                  byte ApplicationStatus, DateTime LastStatusDate,float PaidFees, int CreatedByUserID)>              */
            return (IsFound: isFound , ApplicantPersonID: applicantPersonID , ApplicationDate: applicationDate, ApplicationTypeID: applicationTypeID, ApplicationStatus : applicationStatus,
                    LastStatusDate : lastStatusDate , PaidFees :paidFees , CreatedByUserID : createdByUserID);
        }

        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves all applications using SP_GetAllApplications (which uses the view).
        /// </summary>
        /// <returns>A Task returning a DataTable with application information.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllApplicationsAsync() // Async suffix added
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                // Using the Stored Procedure that selects from the view
                using (SqlCommand command = new SqlCommand("SP_GetAllApplications", connection))
                {
                    command.CommandType = CommandType.StoredProcedure; // Specify it's an SP
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
                        clsEventLogger.LogEvent($"SQL Error in GetAllApplicationsAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetAllApplicationsAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY adds a new application using SP_AddNewApplication.
        /// </summary>
        /// <returns>A Task returning the new ApplicationID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewApplicationAsync(int ApplicantPersonID, DateTime ApplicationDate, int ApplicationTypeID,
             byte ApplicationStatus, DateTime LastStatusDate,
             float PaidFees, int CreatedByUserID) // Async suffix added
        {
            int applicationID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewApplication", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters (matching SP parameter names and types)
                    command.Parameters.AddWithValue("@ApplicantPersonID", ApplicantPersonID);
                    command.Parameters.AddWithValue("@ApplicationDate", ApplicationDate);
                    command.Parameters.AddWithValue("@ApplicationTypeID", ApplicationTypeID);
                    command.Parameters.AddWithValue("@ApplicationStatus", ApplicationStatus); // byte maps to tinyint
                    command.Parameters.AddWithValue("@LastStatusDate", LastStatusDate);
                    command.Parameters.AddWithValue("@PaidFees", PaidFees); // float maps to real
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewApplicationID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            applicationID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in AddNewApplicationAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in AddNewApplicationAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return applicationID;
        }


        /// <summary>
        /// ASYNCHRONOUSLY updates an existing application using SP_UpdateApplication.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateApplicationAsync(int ApplicationID, int ApplicantPersonID, DateTime ApplicationDate, int ApplicationTypeID,
             byte ApplicationStatus, DateTime LastStatusDate,
             float PaidFees, int CreatedByUserID) // Async suffix added
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateApplication", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters (matching SP)
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@ApplicantPersonID", ApplicantPersonID);
                    command.Parameters.AddWithValue("@ApplicationDate", ApplicationDate);
                    command.Parameters.AddWithValue("@ApplicationTypeID", ApplicationTypeID);
                    command.Parameters.AddWithValue("@ApplicationStatus", ApplicationStatus);
                    command.Parameters.AddWithValue("@LastStatusDate", LastStatusDate);
                    command.Parameters.AddWithValue("@PaidFees", PaidFees);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in UpdateApplicationAsync (AppID: {ApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UpdateApplicationAsync (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// ASYNCHRONOUSLY deletes an application by ApplicationID using SP_DeleteApplication.
        /// </summary>
        /// <returns>A Task returning true if deletion was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> DeleteApplicationAsync(int ApplicationID) // Async suffix added
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_DeleteApplication", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in DeleteApplicationAsync (AppID: {ApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in DeleteApplicationAsync (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if an application exists by ApplicationID using SP_IsApplicationExistByID.
        /// </summary>
        /// <returns>A Task returning true if the application exists, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> ApplicationExistsAsync(int ApplicationID) // Renamed and Async suffix added
        {
            bool exists = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_IsApplicationExistByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            exists = Convert.ToBoolean(result);
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in ApplicationExistsAsync (AppID: {ApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in ApplicationExistsAsync (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return exists;
        }

        /// <summary>
        /// ASYNCHRONOUSLY gets the active ApplicationID for a given person and application type using SP_GetActiveApplicationID.
        /// </summary>
        /// <returns>A Task returning the active ApplicationID, or -1 if not found or an error occurs.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> GetActiveApplicationIDAsync(int PersonID, int ApplicationTypeID) // Async suffix added
        {
            int activeApplicationID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetActiveApplicationID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicantPersonID", PersonID);
                    command.Parameters.AddWithValue("@ApplicationTypeID", ApplicationTypeID);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync(); // Expecting a single ID or null

                        if (result != null && result != DBNull.Value)
                        {
                            // Try parsing, default to -1 if parse fails (shouldn't happen with int)
                            int.TryParse(result.ToString(), out activeApplicationID);
                            if (activeApplicationID == 0) activeApplicationID = -1; // Handle case where ID might be 0 if not identity
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetActiveApplicationIDAsync (PersonID: {PersonID}, TypeID: {ApplicationTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        activeApplicationID = -1; // Ensure -1 on error
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetActiveApplicationIDAsync (PersonID: {PersonID}, TypeID: {ApplicationTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        activeApplicationID = -1; // Ensure -1 on error
                        throw; // Re-throw
                    }
                }
            }
            return activeApplicationID;
        }

        /// <summary>
        /// SYNCHRONOUSLY checks if a person has an active application of a specific type.
        /// Calls GetActiveApplicationIDAsync internally with a blocking wait.
        /// </summary>
        /// <returns>True if an active application exists, false otherwise.</returns>
        public static async Task<bool> DoesPersonHaveActiveApplication(int PersonID, int ApplicationTypeID)
        {
            // This method remains synchronous as per BLL structure.
            // It calls the async DAL method but blocks waiting for the result.
            try
            {
                int activeAppID = await GetActiveApplicationIDAsync(PersonID, ApplicationTypeID);
                return (activeAppID != -1);
            }
            catch (Exception ex)
            {
                // Log the error that might have been thrown by GetActiveApplicationIDAsync
                clsEventLogger.LogEvent($"Error checking active application (blocking call) for PersonID {PersonID}, TypeID {ApplicationTypeID}: {ex.Message}", System.Diagnostics.EventLogEntryType.Warning);
                return false; // Assume no active application if error occurs during check
            }
        }


        /// <summary>
        /// ASYNCHRONOUSLY gets the active ApplicationID for a specific person, type, and license class using SP_GetActiveApplicationIDForLicenseClass.
        /// </summary>
        /// <returns>A Task returning the active ApplicationID, or -1 if not found or an error occurs.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> GetActiveApplicationIDForLicenseClassAsync(int PersonID, int ApplicationTypeID, int LicenseClassID) // Async suffix added
        {
            int activeApplicationID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetActiveApplicationIDForLicenseClass", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicantPersonID", PersonID);
                    command.Parameters.AddWithValue("@ApplicationTypeID", ApplicationTypeID);
                    command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID);
                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            int.TryParse(result.ToString(), out activeApplicationID);
                            if (activeApplicationID == 0) activeApplicationID = -1;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetActiveApplicationIDForLicenseClassAsync (PersonID: {PersonID}, TypeID: {ApplicationTypeID}, ClassID: {LicenseClassID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        activeApplicationID = -1;
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetActiveApplicationIDForLicenseClassAsync (PersonID: {PersonID}, TypeID: {ApplicationTypeID}, ClassID: {LicenseClassID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        activeApplicationID = -1;
                        throw; // Re-throw
                    }
                }
            }
            return activeApplicationID;
        }

        /// <summary>
        /// ASYNCHRONOUSLY updates the status of an application using SP_UpdateApplicationStatus.
        /// Automatically sets the LastStatusDate to the current time in the SP.
        /// </summary>
        /// <param name="NewStatus">The new status value (byte/tinyint).</param>
        /// <returns>A Task returning true if the status update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateStatusAsync(int ApplicationID, byte NewStatus) // Renamed and Async suffix added, use byte for TINYINT
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateApplicationStatus", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
                    command.Parameters.AddWithValue("@NewStatus", NewStatus);
                    // LastStatusDate is handled by GETDATE() in the SP now

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in UpdateStatusAsync (AppID: {ApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UpdateStatusAsync (AppID: {ApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

    } // End Class clsApplicationData
} // End Namespace