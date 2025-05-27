using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Required for async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for User entities using Stored Procedures.
    /// Get methods are SYNCHRONOUS to support ref parameters from BLL.
    /// Other methods (Add, Update, Delete, GetAll, IsExist, ChangePassword) are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsUserData
    {
        // --- SYNCHRONOUS Methods (To support ref parameters in BLL.Find) ---

        /// <summary>
        /// SYNCHRONOUSLY retrieves User information by UserID using SP_GetUserInfoByUserID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if the user is found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs general exceptions. Returns false.</exception>
        public static async Task<(bool IsFound, int PersonID, string UserName,string Password, bool IsActive)> GetUserInfoByUserIDAsync(int UserID)
        {
            bool isFound = false;
            int personID = -1; string userName = string.Empty, password = string.Empty; bool isActive = false;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetUserInfoByUserID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserID", UserID);

                    try
                    {
                        await connection.OpenAsync(); // Synchronous Open
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Synchronous Execute
                        {
                            if (await reader.ReadAsync()) // Synchronous Read
                            {
                                isFound = true;
                                // Read data, handling DBNull
                                personID = (int)reader["PersonID"];
                                userName = reader["UserName"] as string ?? string.Empty;
                                password = reader["Password"] as string ?? string.Empty; // Password here is the HASH from DB
                                isActive = (bool)reader["IsActive"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetUserInfoByUserID (UserID: {UserID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false; // Indicate failure
                        // Do not re-throw usually, as BLL expects true/false
                    }
                }
            }
            return (IsFound:isFound , PersonID:personID , UserName:userName , Password:password , IsActive:isActive);
        }

        /// <summary>
        /// SYNCHRONOUSLY retrieves User information by PersonID using SP_GetUserInfoByPersonID.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <returns>True if the user is found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs general exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int UserID, string UserName,string Password, bool IsActive)> GetUserInfoByPersonIDAsync(int PersonID)
        {
            bool isFound = false;
            int userID = -1; string userName = string.Empty, password = string.Empty; bool isActive = false;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetUserInfoByPersonID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", PersonID);

                    try
                    {
                        await connection.OpenAsync(); // Sync Open
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync Execute
                        {
                            if (reader.Read()) // Sync Read
                            {
                                isFound = true;
                                userID = (int)reader["UserID"];
                                userName = reader["UserName"] as string ?? string.Empty;
                                password = reader["Password"] as string ?? string.Empty; // Password here is the HASH from DB
                                isActive = (bool)reader["IsActive"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetUserInfoByPersonID (PersonID: {PersonID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound: isFound, UserID: userID, UserName: userName, Password: password, IsActive: isActive);
        }

        /// <summary>
        /// SYNCHRONOUSLY retrieves User information by Username and Password (Hashed) using SP_GetUserInfoByUsernameAndPassword.
        /// Uses ref parameters. Blocks the calling thread.
        /// </summary>
        /// <param name="PasswordParam">The HASHED password to check against the database.</param>
        /// <returns>True if the user is found, false otherwise.</returns>
        /// <exception cref="Exception">Catches and logs general exceptions. Returns false.</exception>
        public static async Task<(bool IsFound , int UserID, int PersonID, bool IsActive)> GetUserInfoByUsernameAndPasswordAsync(string UserNameParam, string PasswordParam)// PasswordParam is the HASH
        {
            bool isFound = false;
            int userID = -1, personID = -1; bool isActive = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                // Use the SP designed for checking username and HASHED password
                using (SqlCommand command = new SqlCommand("SP_GetUserInfoByUsernameAndPassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", UserNameParam);
                    command.Parameters.AddWithValue("@Password", PasswordParam); // Send the HASH

                    try
                    {
                        await connection.OpenAsync(); // Sync Open
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Sync Execute
                        {
                            if (await reader.ReadAsync()) // Sync Read
                            {
                                isFound = true;
                                userID = (int)reader["UserID"];
                                personID = (int)reader["PersonID"];
                                isActive = (bool)reader["IsActive"];
                                // NOTE: We DO NOT retrieve the password hash again here,
                                // as we already have the necessary info (UserID, PersonID, IsActive).
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"Error in GetUserInfoByUsernameAndPassword (Username: {UserNameParam}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                    }
                }
            }
            return (IsFound:isFound , UserID:userID , PersonID:personID , IsActive:isActive);
        }


        // --- ASYNCHRONOUS Methods ---

        /// <summary>
        /// ASYNCHRONOUSLY adds a new user using SP_AddNewUser.
        /// Expects the password to be already hashed.
        /// </summary>
        /// <param name="passwordHash">The hashed password.</param>
        /// <returns>A Task returning the new UserID, or -1 on failure.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<int> AddNewUserAsync(int PersonID, string UserName,
             string passwordHash, bool IsActive) // Renamed param for clarity
        {
            int userID = -1;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@PersonID", PersonID);
                    command.Parameters.AddWithValue("@UserName", UserName);
                    command.Parameters.AddWithValue("@Password", passwordHash); // Send the hash
                    command.Parameters.AddWithValue("@IsActive", IsActive);

                    // Setup output parameter
                    SqlParameter outputIdParam = new SqlParameter("@NewUserID", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputIdParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get the output value
                        if (outputIdParam.Value != null && outputIdParam.Value != DBNull.Value)
                        {
                            userID = (int)outputIdParam.Value;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in AddNewUserAsync (Username: {UserName}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in AddNewUserAsync (Username: {UserName}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return userID;
        }


        /// <summary>
        /// ASYNCHRONOUSLY updates an existing user using SP_UpdateUser.
        /// Expects the password to be already hashed.
        /// </summary>
        /// <param name="passwordHash">The hashed password.</param>
        /// <returns>A Task returning true if update was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UpdateUserAsync(int UserID, int PersonID, string UserName,
             string passwordHash, bool IsActive) // Renamed param for clarity
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@UserID", UserID);
                    command.Parameters.AddWithValue("@PersonID", PersonID);
                    command.Parameters.AddWithValue("@UserName", UserName);
                    command.Parameters.AddWithValue("@Password", passwordHash); // Send the hash
                    command.Parameters.AddWithValue("@IsActive", IsActive);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in UpdateUserAsync (UserID: {UserID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UpdateUserAsync (UserID: {UserID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }


        /// <summary>
        /// ASYNCHRONOUSLY retrieves all users with their full names using SP_GetAllUsers.
        /// </summary>
        /// <returns>A Task returning a DataTable with user information.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<DataTable> GetAllUsersAsync()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllUsers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                dt.Load(reader); // Sync load is acceptable here
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetAllUsersAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetAllUsersAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// ASYNCHRONOUSLY deletes a user by UserID using SP_DeleteUser.
        /// </summary>
        /// <returns>A Task returning true if deletion was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> DeleteUserAsync(int UserID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_DeleteUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserID", UserID);
                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in DeleteUserAsync (UserID: {UserID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in DeleteUserAsync (UserID: {UserID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if a user exists by UserID using SP_IsUserExistByUserID.
        /// </summary>
        /// <returns>A Task returning true if the user exists, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UserExistsByIDAsync(int UserID) // Renamed
        {
            bool exists = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_IsUserExistByUserID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserID", UserID);
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
                        clsEventLogger.LogEvent($"SQL Error in UserExistsByIDAsync (UserID: {UserID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UserExistsByIDAsync (UserID: {UserID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return exists;
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if a user exists by Username using SP_IsUserExistByUsername.
        /// </summary>
        /// <returns>A Task returning true if the user exists, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UserExistsByUsernameAsync(string UserName) // Renamed
        {
            bool exists = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_IsUserExistByUsername", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
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
                        clsEventLogger.LogEvent($"SQL Error in UserExistsByUsernameAsync (Username: {UserName}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UserExistsByUsernameAsync (Username: {UserName}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return exists;
        }

        /// <summary>
        /// ASYNCHRONOUSLY checks if a user exists for a specific PersonID using SP_IsUserExistByPersonID.
        /// </summary>
        /// <returns>A Task returning true if a user exists for the PersonID, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> UserExistsByPersonIDAsync(int PersonID) // Renamed
        {
            bool exists = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_IsUserExistByPersonID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", PersonID);
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
                        clsEventLogger.LogEvent($"SQL Error in UserExistsByPersonIDAsync (PersonID: {PersonID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UserExistsByPersonIDAsync (PersonID: {PersonID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return exists;
        }

        /// <summary>
        /// ASYNCHRONOUSLY changes the password for a user using SP_ChangePassword.
        /// Expects the new password to be already hashed.
        /// </summary>
        /// <param name="newPasswordHash">The new hashed password.</param>
        /// <returns>A Task returning true if the password change was successful, false otherwise.</returns>
        /// <exception cref="SqlException">Re-throws SQL exceptions from SP.</exception>
        /// <exception cref="Exception">Re-throws general exceptions.</exception>
        public static async Task<bool> ChangePasswordAsync(int UserID, string newPasswordHash) // Renamed & Async
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_ChangePassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserID", UserID);
                    command.Parameters.AddWithValue("@NewPassword", newPasswordHash); // Send the hash
                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in ChangePasswordAsync (UserID: {UserID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in ChangePasswordAsync (UserID: {UserID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }

    } // End Class clsUserData
} // End Namespace