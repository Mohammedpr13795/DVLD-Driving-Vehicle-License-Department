using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for TestAppointment entities using Stored Procedures.
    /// Get methods using ref parameters are SYNCHRONOUS.
    /// Other methods are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsTestAppointmentData
    {
        // --- SYNCHRONOUS Methods (To support BLL.Find/GetLast with ref) ---

        /// <summary>
        /// SYNCHRONOUSLY retrieves Test Appointment information by ID. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int TestTypeID, int LocalDrivingLicenseApplicationID,
            DateTime AppointmentDate, float PaidFees, int CreatedByUserID, bool IsLocked, int RetakeTestApplicationID)> 
            GetTestAppointmentInfoByIDAsync(int TestAppointmentID)
        {
            bool isFound = false;
            int testTypeID = -1, localDrivingLicenseApplicationID = -1, createdByUserID = -1, retakeTestApplicationID = -1;
            DateTime appointmentDate = DateTime.MinValue; float paidFees = -1; bool isLocked = false; 

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetTestAppointmentInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@TestAppointmentID", TestAppointmentID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                testTypeID = (int)reader["TestTypeID"];
                                localDrivingLicenseApplicationID = (int)reader["LocalDrivingLicenseApplicationID"];
                                appointmentDate = (DateTime)reader["AppointmentDate"];
                                paidFees = Convert.ToSingle(reader["PaidFees"]);
                                createdByUserID = (int)reader["CreatedByUserID"];
                                isLocked = (bool)reader["IsLocked"];
                                retakeTestApplicationID = reader["RetakeTestApplicationID"] != DBNull.Value ? (int)reader["RetakeTestApplicationID"] : -1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetTestAppointmentInfoByID (ApptID: {TestAppointmentID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , TestTypeID:testTypeID , LocalDrivingLicenseApplicationID:localDrivingLicenseApplicationID,
                    AppointmentDate:appointmentDate , PaidFees:paidFees , CreatedByUserID:createdByUserID , IsLocked:isLocked , 
                    RetakeTestApplicationID:retakeTestApplicationID);
        }

        /// <summary>
        /// SYNCHRONOUSLY retrieves the last Test Appointment for a specific application and test type. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int TestAppointmentID, DateTime AppointmentDate,
            float PaidFees, int CreatedByUserID, bool IsLocked, int RetakeTestApplicationID)> 
            GetLastTestAppointmentAsync(int LocalDrivingLicenseApplicationID, int TestTypeID)
        {
            bool isFound = false, isLocked = false;
            int testAppointmentID = -1, createdByUserID = -1, retakeTestApplicationID = -1;
            DateTime appointmentDate = DateTime.MinValue;
            float paidFees = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLastTestAppointment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (reader.Read()) // Sync
                            {
                                isFound = true;
                                testAppointmentID = (int)reader["TestAppointmentID"];
                                appointmentDate = (DateTime)reader["AppointmentDate"];
                                paidFees = Convert.ToSingle(reader["PaidFees"]);
                                createdByUserID = (int)reader["CreatedByUserID"];
                                isLocked = (bool)reader["IsLocked"];
                                retakeTestApplicationID = reader["RetakeTestApplicationID"] != DBNull.Value ? (int)reader["RetakeTestApplicationID"] : -1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLastTestAppointment (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , TestAppointmentID:testAppointmentID , AppointmentDate:appointmentDate , PaidFees:paidFees,
                    CreatedByUserID:createdByUserID , IsLocked:isLocked , RetakeTestApplicationID:retakeTestApplicationID);
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves all test appointments using SP_GetAllTestAppointments (uses view).
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllTestAppointmentsAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllTestAppointments", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllTestAppointmentsAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllTestAppointmentsAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY retrieves appointments for a specific application and test type using SP_GetApplicationTestAppointmentsPerTestType.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetApplicationTestAppointmentsPerTestTypeAsync(int LocalDrivingLicenseApplicationID, int TestTypeID) // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetApplicationTestAppointmentsPerTestType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows) { dt.Load(reader); }
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetApplicationTestAppointmentsPerTestTypeAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetApplicationTestAppointmentsPerTestTypeAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, TestType: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY adds a new test appointment using SP_AddNewTestAppointment.
        /// </summary>
        /// <returns>A Task returning the new TestAppointmentID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewTestAppointmentAsync( // Async suffix
             int TestTypeID, int LocalDrivingLicenseApplicationID,
             DateTime AppointmentDate, float PaidFees, int CreatedByUserID, int RetakeTestApplicationID)
        {
            int testAppointmentID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewTestAppointment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@AppointmentDate", AppointmentDate);
                    command.Parameters.AddWithValue("@PaidFees", PaidFees);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);
                    command.Parameters.AddWithValue("@RetakeTestApplicationID", RetakeTestApplicationID == -1 ? (object)DBNull.Value : RetakeTestApplicationID); // Handle -1 as NULL

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewTestAppointmentID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            testAppointmentID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in AddNewTestAppointmentAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, Type: {TestTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in AddNewTestAppointmentAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}, Type: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return testAppointmentID;
        }

        /// <summary>
        /// ASYNCHRONOUSLY updates an existing test appointment using SP_UpdateTestAppointment.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateTestAppointmentAsync(int TestAppointmentID, int TestTypeID, int LocalDrivingLicenseApplicationID, // Async suffix
             DateTime AppointmentDate, float PaidFees,
             int CreatedByUserID, bool IsLocked, int RetakeTestApplicationID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateTestAppointment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@TestAppointmentID", TestAppointmentID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
                    command.Parameters.AddWithValue("@AppointmentDate", AppointmentDate);
                    command.Parameters.AddWithValue("@PaidFees", PaidFees);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);
                    command.Parameters.AddWithValue("@IsLocked", IsLocked);
                    command.Parameters.AddWithValue("@RetakeTestApplicationID", RetakeTestApplicationID == -1 ? (object)DBNull.Value : RetakeTestApplicationID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in UpdateTestAppointmentAsync (ApptID: {TestAppointmentID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in UpdateTestAppointmentAsync (ApptID: {TestAppointmentID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }


        /// <summary>
        /// ASYNCHRONOUSLY gets the TestID associated with a specific TestAppointmentID using SP_GetTestIDByAppointmentID.
        /// </summary>
        /// <returns>A Task returning the TestID, or -1 if not found or error occurs.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> GetTestIDAsync(int TestAppointmentID) // Async suffix
        {
            int testID = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetTestIDByAppointmentID", connection)) // Use the correct SP name
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@TestAppointmentID", TestAppointmentID);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            int.TryParse(result.ToString(), out testID);
                            if (testID == 0) testID = -1; // Handle non-identity if necessary
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetTestIDAsync (ApptID: {TestAppointmentID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); testID = -1; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetTestIDAsync (ApptID: {TestAppointmentID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); testID = -1; throw; }
                }
            }
            return testID;
        }

    } // End Class clsTestAppointmentData
} // End Namespace