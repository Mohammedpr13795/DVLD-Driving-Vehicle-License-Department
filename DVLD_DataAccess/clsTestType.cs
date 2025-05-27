using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for TestType entities using Stored Procedures.
    /// GetTestTypeInfoByID is SYNCHRONOUS to support ref parameters from BLL.
    /// Other methods (GetAll, Add, Update) are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsTestTypeData
    {
        // --- SYNCHRONOUS Method (To support BLL.Find with ref) ---

        /// <summary>
        /// SYNCHRONOUSLY retrieves Test Type information by ID using SP_GetTestTypeInfoByID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , string TestTypeTitle, string TestDescription, float TestFees)>
            GetTestTypeInfoByIDAsync(int TestTypeID)
        {
            bool isFound = false;
            string testTypeTitle = string.Empty, testDescription = string.Empty; float testFees = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetTestTypeInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                testTypeTitle = reader["TestTypeTitle"] as string ?? string.Empty;
                                testDescription = reader["TestTypeDescription"] as string ?? string.Empty; // Correct column name
                                testFees = Convert.ToSingle(reader["TestTypeFees"]); // Correct column name
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetTestTypeInfoByID (ID: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , TestTypeTitle:testTypeTitle , TestDescription:testDescription , TestFees:testFees);
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves all test types using SP_GetAllTestTypes.
        /// </summary>
        /// <returns>A Task returning a DataTable with all test types.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllTestTypesAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllTestTypes", connection))
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
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in GetAllTestTypesAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in GetAllTestTypesAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY adds a new test type using SP_AddNewTestType.
        /// </summary>
        /// <param name="Title">The title of the new test type.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Fees">The fees.</param>
        /// <returns>A Task returning the new TestTypeID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewTestTypeAsync(string Title, string Description, float Fees) // Async suffix
        {
            int testTypeID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewTestType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters (matching SP)
                    command.Parameters.AddWithValue("@TestTypeTitle", Title);
                    command.Parameters.AddWithValue("@TestTypeDescription", (object)Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TestTypeFees", Fees);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewTestTypeID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            testTypeID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in AddNewTestTypeAsync (Title: {Title}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in AddNewTestTypeAsync (Title: {Title}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return testTypeID;
        }

        /// <summary>
        /// ASYNCHRONOUSLY updates an existing test type using SP_UpdateTestType.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateTestTypeAsync(int TestTypeID, string Title, string Description, float Fees) // Async suffix
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateTestType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters (matching SP)
                    command.Parameters.AddWithValue("@TestTypeID", TestTypeID);
                    command.Parameters.AddWithValue("@TestTypeTitle", Title);
                    command.Parameters.AddWithValue("@TestTypeDescription", (object)Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TestTypeFees", Fees);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) { clsEventLogger.LogEvent($"SQL Error in UpdateTestTypeAsync (ID: {TestTypeID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error); throw; }
                    catch (Exception ex) { clsEventLogger.LogEvent($"General Error in UpdateTestTypeAsync (ID: {TestTypeID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error); throw; }
                }
            }
            return (rowsAffected > 0);
        }

    } // End Class clsTestTypeData
} // End Namespace