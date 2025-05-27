using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for LicenseClass entities using Stored Procedures.
    /// Get methods using ref parameters are ASYNCHRONOUS.
    /// Other methods (GetAll, Add, Update, GetScalar) are AASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsLicenseClassData
    {
        // --- ASYNCHRONOUS Methods (To support BLL.Find with ref) ---

        /// <summary>
        /// ASYNCHRONOUSLY retrieves License Class information by ID using SP_GetLicenseClassInfoByID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static async Task<(bool IsFound,string ClassName, string ClassDescription, byte MinimumAllowedAge,
            byte DefaultValidityLength, float ClassFees)> GetLicenseClassInfoByIDAsync(int LicenseClassID)
        {
            bool isFound = false;
            string className = string.Empty, classDescription = string.Empty;
            byte minimumAllowedAge = 0, defaultValidityLength = 0;
            float classFees = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLicenseClassInfoByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                className = reader["ClassName"] as string ?? string.Empty;
                                classDescription = reader["ClassDescription"] as string ?? string.Empty;
                                minimumAllowedAge = (byte)reader["MinimumAllowedAge"];
                                defaultValidityLength = (byte)reader["DefaultValidityLength"];
                                classFees = Convert.ToSingle(reader["ClassFees"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLicenseClassInfoByID (ID: {LicenseClassID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , ClassName:className , ClassDescription:classDescription,
                    MinimumAllowedAge:minimumAllowedAge, DefaultValidityLength:defaultValidityLength, ClassFees:classFees);
        }

        public static async Task<(bool IsFound, int LicenseClassID, string ClassDescription, byte MinimumAllowedAge,
    byte DefaultValidityLength, float ClassFees)> GetLicenseClassInfoByClassNameAsync(string ClassNameParam)
        {
            bool isFound = false;
            int licenseClassID = -1;
            string classDescription = string.Empty;
            byte minimumAllowedAge = 0, defaultValidityLength = 0;
            float classFees = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLicenseClassInfoByClassName", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ClassName", ClassNameParam);

                    try
                    {
                        await connection.OpenAsync(); // Sync
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync
                        {
                            if (await reader.ReadAsync()) // Sync
                            {
                                isFound = true;
                                licenseClassID = (int)reader["LicenseClassID"];
                                classDescription = reader["ClassDescription"] as string ?? string.Empty;
                                minimumAllowedAge = (byte)reader["MinimumAllowedAge"];
                                defaultValidityLength = (byte)reader["DefaultValidityLength"];
                                classFees = Convert.ToSingle(reader["ClassFees"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLicenseClassInfoByID (ID: {licenseClassID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound: isFound, LicenseClassID: licenseClassID, ClassDescription: classDescription,
                    MinimumAllowedAge: minimumAllowedAge, DefaultValidityLength: defaultValidityLength, ClassFees: classFees);
        }

        /// <summary>
        /// ASYNCHRONOUSLY retrieves License Class information by Class Name using SP_GetLicenseClassInfoByClassName.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs exceptions. Returns false.</exception>
        public static bool GetLicenseClassInfoByClassName(string ClassName, ref int LicenseClassID,
            ref string ClassDescription, ref byte MinimumAllowedAge,
           ref byte DefaultValidityLength, ref float ClassFees)
        {
            bool isFound = false;
            LicenseClassID = -1; // Initialize ref output

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetLicenseClassInfoByClassName", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ClassName", ClassName);

                    try
                    {
                        connection.Open(); // Sync
                        using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow)) // Sync
                        {
                            if (reader.Read()) // Sync
                            {
                                isFound = true;
                                LicenseClassID = (int)reader["LicenseClassID"];
                                ClassDescription = reader["ClassDescription"] as string ?? string.Empty;
                                MinimumAllowedAge = (byte)reader["MinimumAllowedAge"];
                                DefaultValidityLength = (byte)reader["DefaultValidityLength"];
                                ClassFees = Convert.ToSingle(reader["ClassFees"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetLicenseClassInfoByClassName (Name: {ClassName}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return isFound;
        }


        // --- AASYNCHRONOUS Methods ---

        /// <summary>
        /// AASYNCHRONOUSLY retrieves all license classes using SP_GetAllLicenseClasses.
        /// </summary>
        /// <returns>A Task returning a DataTable with all license classes.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllLicenseClassesAsync() // Async suffix
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllLicenseClasses", connection))
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
                        clsEventLogger.LogEvent($"SQL Error in GetAllLicenseClassesAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetAllLicenseClassesAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// AASYNCHRONOUSLY adds a new license class using SP_AddNewLicenseClass.
        /// </summary>
        /// <returns>A Task returning the new LicenseClassID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewLicenseClassAsync(string ClassName, string ClassDescription,
            byte MinimumAllowedAge, byte DefaultValidityLength, float ClassFees) // Async suffix
        {
            int licenseClassID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewLicenseClass", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters (matching SP)
                    command.Parameters.AddWithValue("@ClassName", ClassName);
                    command.Parameters.AddWithValue("@ClassDescription", (object)ClassDescription ?? DBNull.Value); // Handle potential null description
                    command.Parameters.AddWithValue("@MinimumAllowedAge", MinimumAllowedAge);
                    command.Parameters.AddWithValue("@DefaultValidityLength", DefaultValidityLength);
                    command.Parameters.AddWithValue("@ClassFees", ClassFees);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewLicenseClassID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            licenseClassID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in AddNewLicenseClassAsync (Name: {ClassName}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in AddNewLicenseClassAsync (Name: {ClassName}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return licenseClassID;
        }

        /// <summary>
        /// AASYNCHRONOUSLY updates an existing license class using SP_UpdateLicenseClass.
        /// </summary>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateLicenseClassAsync(int LicenseClassID, string ClassName,
            string ClassDescription, byte MinimumAllowedAge, byte DefaultValidityLength, float ClassFees) // Async suffix
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateLicenseClass", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters (matching SP)
                    command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID);
                    command.Parameters.AddWithValue("@ClassName", ClassName);
                    command.Parameters.AddWithValue("@ClassDescription", (object)ClassDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MinimumAllowedAge", MinimumAllowedAge);
                    command.Parameters.AddWithValue("@DefaultValidityLength", DefaultValidityLength);
                    command.Parameters.AddWithValue("@ClassFees", ClassFees);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in UpdateLicenseClassAsync (ID: {LicenseClassID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UpdateLicenseClassAsync (ID: {LicenseClassID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// AASYNCHRONOUSLY gets the minimum allowed age for a specific license class ID using SP_GetMinimumAllowedAgeByID.
        /// </summary>
        /// <param name="ID">The LicenseClassID.</param>
        /// <returns>A Task returning the minimum age, or -1 if not found or error occurs.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> GetMinimumAllowedAgeByIDAsync(int ID) // Async suffix
        {
            int minimumAllowedAge = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetMinimumAllowedAgeByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LicenseClassID", ID);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            // MinimumAllowedAge is TINYINT (byte), convert carefully
                            minimumAllowedAge = Convert.ToInt32(result);
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetMinimumAllowedAgeByIDAsync (ID: {ID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        minimumAllowedAge = -1; // Ensure default on error
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetMinimumAllowedAgeByIDAsync (ID: {ID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        minimumAllowedAge = -1; // Ensure default on error
                        throw; // Re-throw
                    }
                }
            }
            return minimumAllowedAge;
        }

        /// <summary>
        /// AASYNCHRONOUSLY gets the class name for a specific license class ID using SP_GetClassNameByID.
        /// </summary>
        /// <param name="ID">The LicenseClassID.</param>
        /// <returns>A Task returning the class name, or null if not found or error occurs.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<string> GetClassNameByIDAsync(int ID) // Async suffix
        {
            string className = null; // Use null for string default
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetClassNameByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LicenseClassID", ID);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            className = result.ToString();
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetClassNameByIDAsync (ID: {ID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        className = null; // Ensure null on error
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetClassNameByIDAsync (ID: {ID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        className = null; // Ensure null on error
                        throw; // Re-throw
                    }
                }
            }
            return className;
        }

    } // End Class clsLicenseClassData
} // End Namespace