using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for ApplicationType entities using Stored Procedures.
    /// GetApplicationTypeInfoByID is SYNCHRONOUS to support ref parameters from BLL.
    /// Other methods (GetAll, Add, Update) are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsApplicationTypeData
    {
        // --- SYNCHRONOUS Method (To support BLL.Find) ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves Application Type information by ID using SP_GetApplicationTypeInfoByID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <param name="ApplicationTypeID">The ID of the application type to retrieve.</param>
        /// <param name="ApplicationTypeTitle">Reference parameter to store the title.</param>
        /// <param name="ApplicationFees">Reference parameter to store the fees.</param>
        /// <returns>True if the application type is found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs general exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , string ApplicationTypeTitle, float ApplicationFees)> 
            GetApplicationTypeInfoByIDAsync(int ApplicationTypeID)
        {
            bool isFound = false;
            string applicationTypeTitle = string.Empty ;
            float applicationFees = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetApplicationTypeInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApplicationTypeID", ApplicationTypeID);

                    try
                    {
                        await connection.OpenAsync(); // Sync Open
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync Execute
                        {
                            if (await reader.ReadAsync()) // Sync Read
                            {
                                isFound = true;
                                applicationTypeTitle = reader["ApplicationTypeTitle"] as string ?? string.Empty;
                                // Use Convert.ToSingle for REAL/FLOAT types
                                applicationFees = reader["ApplicationFees"] != DBNull.Value ? Convert.ToSingle(reader["ApplicationFees"]) : 0f;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetApplicationTypeInfoByID (ID: {ApplicationTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound: isFound , ApplicationTypeTitle:applicationTypeTitle, ApplicationFees:applicationFees);
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// AASYNCHRONOUSLY retrieves all application types using SP_GetAllApplicationTypes.
        /// </summary>
        /// <returns>A Task returning a DataTable with all application types.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllApplicationTypesAsync() // Async suffix added
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllApplicationTypes", connection))
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
                        clsEventLogger.LogEvent($"SQL Error in GetAllApplicationTypesAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetAllApplicationTypesAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// AASYNCHRONOUSLY adds a new application type using SP_AddNewApplicationType.
        /// </summary>
        /// <param name="Title">The title of the new application type.</param>
        /// <param name="Fees">The fees for the new application type.</param>
        /// <returns>A Task returning the new ApplicationTypeID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewApplicationTypeAsync(string Title, float Fees) // Async suffix added
        {
            int applicationTypeID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewApplicationType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters (matching SP)
                    command.Parameters.AddWithValue("@ApplicationTypeTitle", Title);
                    command.Parameters.AddWithValue("@ApplicationFees", Fees); // float maps to REAL

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewApplicationTypeID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            applicationTypeID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in AddNewApplicationTypeAsync (Title: {Title}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in AddNewApplicationTypeAsync (Title: {Title}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return applicationTypeID;
        }

        /// <summary>
        /// AASYNCHRONOUSLY updates an existing application type using SP_UpdateApplicationType.
        /// </summary>
        /// <param name="ApplicationTypeID">The ID of the type to update.</param>
        /// <param name="Title">The new title.</param>
        /// <param name="Fees">The new fees.</param>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateApplicationTypeAsync(int ApplicationTypeID, string Title, float Fees) // Async suffix added
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateApplicationType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters (matching SP)
                    command.Parameters.AddWithValue("@ApplicationTypeID", ApplicationTypeID);
                    command.Parameters.AddWithValue("@ApplicationTypeTitle", Title);
                    command.Parameters.AddWithValue("@ApplicationFees", Fees);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in UpdateApplicationTypeAsync (ID: {ApplicationTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UpdateApplicationTypeAsync (ID: {ApplicationTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

    } // End Class clsApplicationTypeData
} // End Namespace