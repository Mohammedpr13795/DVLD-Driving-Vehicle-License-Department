using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for Driver entities using Stored Procedures.
    /// Get methods using ref parameters are ASYNCHRONOUS.
    /// Other methods (GetAll, Add, Update) are AASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsDriverData
    {
        // --- ASYNCHRONOUS Methods (To support BLL.Find with ref) ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves Driver information by DriverID using SP_GetDriverInfoByDriverID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound,int PersonID, int CreatedByUserID, DateTime CreatedDate)> 
            GetDriverInfoByDriverIDAsync(int DriverID)
        {
            bool isFound = false;
            int personID = -1, createdByUserID = -1;
            DateTime createdDate = DateTime.MinValue;
            

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetDriverInfoByDriverID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DriverID", DriverID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                personID = (int)reader["PersonID"];
                                createdByUserID = (int)reader["CreatedByUserID"];
                                createdDate = (DateTime)reader["CreatedDate"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetDriverInfoByDriverID (DriverID: {DriverID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound, PersonID:personID, CreatedByUserID:createdByUserID, CreatedDate:createdDate);
        }

        /// <summary>
        /// ASYNCHRONOUSLY retrieves Driver information by PersonID using SP_GetDriverInfoByPersonID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int DriverID,int CreatedByUserID, DateTime CreatedDate)> GetDriverInfoByPersonIDAsync(int PersonID)
        {
            bool isFound = false;
            int driverID = -1, createdByUserID = -1;
            DateTime createdDate = DateTime.MinValue;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetDriverInfoByPersonID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", PersonID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (reader.Read()) // Sync
                            {
                                isFound = true;
                                driverID = (int)reader["DriverID"];
                                createdByUserID = (int)reader["CreatedByUserID"];
                                createdDate = (DateTime)reader["CreatedDate"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetDriverInfoByPersonID (PersonID: {PersonID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , DriverID:driverID , CreatedByUserID:createdByUserID , CreatedDate:createdDate);
        }


        // --- AASYNCHRONOUS Methods ---

        /// <summary>
        /// AASYNCHRONOUSLY retrieves all drivers using SP_GetAllDrivers (which uses Drivers_View).
        /// </summary>
        /// <returns>A Task returning a DataTable with driver information.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllDriversAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                // Use SP that selects from the view
                using (SqlCommand command = new SqlCommand("SP_GetAllDrivers", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllDriversAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllDriversAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// AASYNCHRONOUSLY adds a new driver record using SP_AddNewDriver.
        /// CreatedDate is set by the SP using GETDATE().
        /// </summary>
        /// <returns>A Task returning the new DriverID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewDriverAsync(int PersonID, int CreatedByUserID) // Async suffix
        {
            int driverID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewDriver", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@PersonID", PersonID);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);
                    // @CreatedDate is handled by SP (using GETDATE())

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewDriverID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            driverID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in AddNewDriverAsync (PersonID: {PersonID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in AddNewDriverAsync (PersonID: {PersonID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return driverID;
        }

        /// <summary>
        /// AASYNCHRONOUSLY updates an existing driver record using SP_UpdateDriver.
        /// Note: Typically CreatedDate and CreatedByUserID are not updated after creation.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateDriverAsync(int DriverID, int PersonID, int CreatedByUserID) // Async suffix
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateDriver", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@DriverID", DriverID);
                    command.Parameters.AddWithValue("@PersonID", PersonID);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in UpdateDriverAsync (DriverID: {DriverID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in UpdateDriverAsync (DriverID: {DriverID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }

    } // End Class clsDriverData
} // End Namespace