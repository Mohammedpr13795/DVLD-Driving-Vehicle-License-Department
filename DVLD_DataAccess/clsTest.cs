using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for Test entities using Stored Procedures.
    /// Get methods using ref parameters are SYNCHRONOUS.
    /// Other methods are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsTestData
    {
        // --- SYNCHRONOUS Methods (To support BLL.Find with ref) ---

        /// <summary>
        /// SYNCHRONOUSLY retrieves Test information by ID using SP_GetTestInfoByID. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound,int TestAppointmentID, bool TestResult,string Notes, int CreatedByUserID)> 
            GetTestInfoByIDAsync(int TestID)
        {
            bool isFound = false , testResult = false;
            int testAppointmentID = -1, createdByUserID = -1; 
            string notes = string.Empty;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetTestInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@TestID", TestID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                testAppointmentID = (int)reader["TestAppointmentID"];
                                testResult = (bool)reader["TestResult"];
                                notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null;
                                createdByUserID = (int)reader["CreatedByUserID"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetTestInfoByID (TestID: {TestID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , TestAppointmentID:testAppointmentID , TestResult:testResult , Notes:notes , CreatedByUserID:createdByUserID);
        }

        /// <summary>
        /// SYNCHRONOUSLY retrieves the last test for a specific person, license class, and test type using SP_GetLastTestByPersonAndTestTypeAndLicenseClass. Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int TestID,int TestAppointmentID, bool TestResult,
                                    string Notes, int CreatedByUserID)> 
            GetLastTestByPersonAndTestTypeAndLicenseClassAsync(int PersonID, int LicenseClassID, int TestTypeID)
        {
            bool isFound = false, testResult = false;
            int testID = -1, testAppointmentID = -1, createdByUserID = -1;
            string notes = string.Empty;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLastTestByPersonAndTestTypeAndLicenseClass", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", PersonID);
                    command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID);
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                testID = (int)reader["TestID"];
                                testAppointmentID = (int)reader["TestAppointmentID"];
                                testResult = (bool)reader["TestResult"];
                                notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null;
                                createdByUserID = (int)reader["CreatedByUserID"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLastTestByPersonAndTestTypeAndLicenseClass (PersonID: {PersonID}, Class: {LicenseClassID}, Type: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , TestID:testID , TestAppointmentID:testAppointmentID , 
                TestResult:testResult , Notes:notes , CreatedByUserID:createdByUserID);
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves all tests using SP_GetAllTests.
        /// </summary>
        /// <returns>A Task returning a DataTable.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllTestsAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllTests", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllTestsAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllTestsAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY adds a new test record and locks the appointment using SP_AddNewTest.
        /// </summary>
        /// <returns>A Task returning the new TestID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewTestAsync(int TestAppointmentID, bool TestResult, // Async suffix
             string Notes, int CreatedByUserID)
        {
            int testID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewTest", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@TestAppointmentID", TestAppointmentID);
                    command.Parameters.AddWithValue("@TestResult", TestResult);
                    command.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(Notes) ? (object)DBNull.Value : Notes);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewTestID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync(); // SP handles transaction

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            testID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in AddNewTestAsync (ApptID: {TestAppointmentID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in AddNewTestAsync (ApptID: {TestAppointmentID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return testID;
        }

        /// <summary>
        /// ASYNCHRONOUSLY updates an existing test record using SP_UpdateTest.
        /// Note: Updating test results is generally unusual.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateTestAsync(int TestID, int TestAppointmentID, bool TestResult, // Async suffix
             string Notes, int CreatedByUserID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateTest", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@TestID", TestID);
                    command.Parameters.AddWithValue("@TestAppointmentID", TestAppointmentID);
                    command.Parameters.AddWithValue("@TestResult", TestResult);
                    command.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(Notes) ? (object)DBNull.Value : Notes);
                    command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in UpdateTestAsync (TestID: {TestID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in UpdateTestAsync (TestID: {TestID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// ASYNCHRONOUSLY gets the count of passed tests for a specific LocalDrivingLicenseApplicationID using SP_GetPassedTestCount.
        /// </summary>
        /// <returns>A Task returning the count of passed tests (byte), or 0 on error.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<byte> GetPassedTestCountAsync(int LocalDrivingLicenseApplicationID) // Async suffix
        {
            byte passedTestCount = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetPassedTestCount", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            // SP returns INT, convert carefully to byte
                            int countInt = Convert.ToInt32(result);
                            passedTestCount = (countInt > byte.MaxValue) ? byte.MaxValue : Convert.ToByte(countInt);
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetPassedTestCountAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); passedTestCount = 0; throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetPassedTestCountAsync (LDLA_ID: {LocalDrivingLicenseApplicationID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); passedTestCount = 0; throw; }
                }
            }
            return passedTestCount;
        }

    } // End Class clsTestData
} // End Namespace