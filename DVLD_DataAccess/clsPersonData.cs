using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks; // Still needed for other async methods

namespace DVLD_DataAccess
{
    /// <summary>
    /// Provides data access functionalities for Person entities using Stored Procedures.
    /// Get methods are SYNCHRONOUS to support ref parameters from BLL.
    /// Other methods (Add, Update, Delete, Exists, GetAll) are ASYNCHRONOUS.
    /// Conforms to C# 7.3 (.NET Framework 4.8).
    /// </summary>
    public static class clsPersonData
    {
        /// <summary>
        /// SYNCHRONOUSLY retrieves Person information by PersonID using SP_GetPersonByID.
        /// Uses ref parameters as required by the original BLL call signature.
        /// </summary>
        /// <returns>True if the person is found, false otherwise.</returns>
        /// <exception cref="SqlException">Thrown on database access errors.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<(bool IsFound , string FirstName, string SecondName,
          string ThirdName, string LastName, string NationalNo, DateTime DateOfBirth,
           short Gendor, string Address, string Phone, string Email,
           int NationalityCountryID, string ImagePath)> GetPersonInfoByIDAsync(int PersonID )
        {
            bool isFound = false;
            string firstName = string.Empty, secondName = string.Empty, thirdName = string.Empty, lastName = string.Empty, nationalNo = string.Empty,
            address = string.Empty, phone = string.Empty, email = string.Empty, imagePath = string.Empty;
            DateTime dateOfBirth = DateTime.MinValue;
            short gendor = 0; int nationalityCountryID = -1;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetPersonByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", PersonID);

                    try
                    {
                        await connection.OpenAsync(); // Synchronous Open
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Synchronous ExecuteReader
                        {
                            if (await reader.ReadAsync()) // Synchronous Read
                            {
                                // Record found
                                isFound = true;

                                // Read data, handling DBNull
                                firstName = reader["FirstName"] as string ?? string.Empty;
                                secondName = reader["SecondName"] as string ?? string.Empty;
                                thirdName = reader["ThirdName"] != DBNull.Value ? reader["ThirdName"] as string : null;
                                lastName = reader["LastName"] as string ?? string.Empty;
                                nationalNo = reader["NationalNo"] as string ?? string.Empty;
                                dateOfBirth = reader["DateOfBirth"] != DBNull.Value ? (DateTime)reader["DateOfBirth"] : DateTime.MinValue;
                                gendor = reader["Gendor"] != DBNull.Value ? Convert.ToInt16(reader["Gendor"]) : (short)-1;
                                address = reader["Address"] as string ?? string.Empty;
                                phone = reader["Phone"] as string ?? string.Empty;
                                email = reader["Email"] != DBNull.Value ? reader["Email"] as string : null;
                                nationalityCountryID = reader["NationalityCountryID"] != DBNull.Value ? (int)reader["NationalityCountryID"] : -1;
                                imagePath = reader["ImagePath"] != DBNull.Value ? reader["ImagePath"] as string : null;
                            }
                            // else: not found, isFound remains false.
                        } // reader disposed here
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetPersonInfoByID (ID: {PersonID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false; // Ensure false on error
                        // Consider re-throwing if BLL needs to handle it:
                        // throw;
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetPersonInfoByID (ID: {PersonID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false; // Ensure false on error
                        // Consider re-throwing:
                        // throw;
                    }
                    // 'using' handles connection closing even on exceptions
                }
            }
            return (IsFound: isFound, FirstName: firstName, SecondName: secondName, ThirdName: thirdName,
                    LastName: lastName, NationalNo: nationalNo, DateOfBirth: dateOfBirth, Gendor: gendor,
                    Address: address, Phone: phone, Email: email, NationalityCountryID: nationalityCountryID, ImagePath: imagePath);
        }


        /// <summary>
        /// SYNCHRONOUSLY retrieves Person information by NationalNo using SP_GetPersonByNationalNo.
        /// Uses ref parameters as required by the original BLL call signature.
        /// </summary>
        /// <returns>True if the person is found, false otherwise.</returns>
        /// <exception cref="SqlException">Thrown on database access errors.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<(bool IsFound, int PersonID, string FirstName, string SecondName,
            string ThirdName, string LastName, DateTime DateOfBirth,
             short Gendor, string Address, string Phone, string Email,
             int NationalityCountryID, string ImagePath)>
            GetPersonInfoByNationalNoAsync(string NationalNoParam)
        {
            bool isFound = false;
            int personID = -1; string firstName = string.Empty, secondName = string.Empty,
            thirdName = string.Empty, lastName = string.Empty;  DateTime dateOfBirth = DateTime.MinValue;
            short gendor = 0; string address = string.Empty, phone = string.Empty, email = string.Empty;
            int nationalityCountryID = -1; string imagePath = string.Empty;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetPersonByNationalNo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@NationalNo", NationalNoParam);

                    try
                    {
                        await connection.OpenAsync(); // Synchronous Open
                        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) // Synchronous ExecuteReader
                        {
                            if (await reader.ReadAsync()) // Synchronous Read
                            {
                                isFound = true;
                                // Read data, handling DBNull
                                personID = (int)reader["PersonID"];
                                firstName = reader["FirstName"] as string ?? string.Empty;
                                secondName = reader["SecondName"] as string ?? string.Empty;
                                thirdName = reader["ThirdName"] != DBNull.Value ? reader["ThirdName"] as string : null;
                                lastName = reader["LastName"] as string ?? string.Empty;
                                dateOfBirth = reader["DateOfBirth"] != DBNull.Value ? (DateTime)reader["DateOfBirth"] : DateTime.MinValue;
                                gendor = reader["Gendor"] != DBNull.Value ? Convert.ToInt16(reader["Gendor"]) : (short)-1;
                                address = reader["Address"] as string ?? string.Empty;
                                phone = reader["Phone"] as string ?? string.Empty;
                                email = reader["Email"] != DBNull.Value ? reader["Email"] as string : null;
                                nationalityCountryID = reader["NationalityCountryID"] != DBNull.Value ? (int)reader["NationalityCountryID"] : -1;
                                imagePath = reader["ImagePath"] != DBNull.Value ? reader["ImagePath"] as string : null;
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetPersonInfoByNationalNo (NatNo: {NationalNoParam}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                        // throw; // Optional re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetPersonInfoByNationalNo (NatNo: {NationalNoParam}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        isFound = false;
                        // throw; // Optional re-throw
                    }
                }
            }
            return (IsFound:isFound, PersonID:personID, FirstName:firstName, SecondName:secondName,
            ThirdName:thirdName, LastName:lastName, DateOfBirth:dateOfBirth,
             Gendor:gendor, Address:address, Phone:phone, Email:email,
             NationalityCountryID:nationalityCountryID, ImagePath:imagePath);
        }


        // --- ASYNCHRONOUS Methods (Add, Update, GetAll, Delete, Exists) ---
        // --- These remain unchanged from the previous correct versions ---

        /// <summary>
        /// Asynchronously adds a new person using the SP_AddNewPerson stored procedure.
        /// This method IS fully asynchronous (returns Task<int>).
        /// </summary>
        /// <returns>A Task returning the ID (int) of the newly added person, or -1 if the SP indicates failure.</returns>
        /// <exception cref="SqlException">Thrown on database errors if SP uses THROW.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<int> AddNewPersonAsync(string firstName, string secondName, string thirdName,
            string lastName, string nationalNo, DateTime dateOfBirth, short gendor, string address,
            string phone, string email, int nationalityCountryID, string imagePath)
        {
            int personID = -1; // Default to failure indication

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewPerson", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@SecondName", secondName);
                    command.Parameters.AddWithValue("@ThirdName", string.IsNullOrEmpty(thirdName) ? (object)DBNull.Value : thirdName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@NationalNo", nationalNo);
                    command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                    command.Parameters.AddWithValue("@Gendor", gendor);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@Phone", phone);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    command.Parameters.AddWithValue("@NationalityCountryID", nationalityCountryID);
                    command.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);

                    // Setup the OUTPUT parameter
                    SqlParameter outputParameter = new SqlParameter("@NewPersonID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParameter);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync(); // Execute asynchronously

                        // Get output value
                        if (outputParameter.Value != null && outputParameter.Value != DBNull.Value)
                        {
                            personID = (int)outputParameter.Value;
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in AddNewPersonAsync (NatNo: {nationalNo}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw to BLL
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in AddNewPersonAsync (NatNo: {nationalNo}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw to BLL
                    }
                }
            }
            return personID;
        }


        /// <summary>
        /// Asynchronously updates an existing person using the SP_UpdatePerson stored procedure.
        /// This method IS fully asynchronous (returns Task<bool>).
        /// </summary>
        /// <returns>A Task returning true if the update was successful (at least one row affected); otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown on database errors if SP uses THROW.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<bool> UpdatePersonAsync(int personID, string firstName, string secondName, string thirdName,
            string lastName, string nationalNo, DateTime dateOfBirth, short gendor, string address,
            string phone, string email, int nationalityCountryID, string imagePath)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_UpdatePerson", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@PersonID", personID);
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@SecondName", secondName);
                    command.Parameters.AddWithValue("@ThirdName", string.IsNullOrEmpty(thirdName) ? (object)DBNull.Value : thirdName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@NationalNo", nationalNo);
                    command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                    command.Parameters.AddWithValue("@Gendor", gendor);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@Phone", phone);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    command.Parameters.AddWithValue("@NationalityCountryID", nationalityCountryID);
                    command.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync(); // Execute update asynchronously
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in UpdatePersonAsync (ID: {personID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in UpdatePersonAsync (ID: {personID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0); // Return true if rows were affected
        }


        /// <summary>
        /// Asynchronously retrieves all people using the SP_GetAllPeople stored procedure.
        /// This method IS fully asynchronous (returns Task<DataTable>).
        /// </summary>
        /// <returns>A Task returning a DataTable populated with all people data.</returns>
        /// <exception cref="SqlException">Thrown on database errors if SP uses THROW.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<DataTable> GetAllPeopleAsync()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllPeople", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                dt.Load(reader); // Load synchronously
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        clsEventLogger.LogEvent($"SQL Error in GetAllPeopleAsync: {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in GetAllPeopleAsync: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return dt;
        }


        /// <summary>
        /// Asynchronously deletes a person using the SP_DeletePerson stored procedure.
        /// This method IS fully asynchronous (returns Task<bool>).
        /// </summary>
        /// <returns>A Task returning true if deletion was successful; otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown on database errors if SP uses THROW.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<bool> DeletePersonAsync(int personID)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_DeletePerson", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", personID);

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                    //catch (SqlException ex)
                    //{
                    //    clsEventLogger.LogEvent($"SQL Error in DeletePersonAsync (ID: {personID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                    //    throw; // Re-throw
                    //}
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in DeletePersonAsync (ID: {personID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return (rowsAffected > 0);
        }


        /// <summary>
        /// Asynchronously checks if a person exists by PersonID using SP_CheckPersonExistsByID.
        /// This method IS fully asynchronous (returns Task<bool>).
        /// </summary>
        /// <returns>A Task returning true if the person exists; otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown on database errors if SP uses THROW.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<bool> PersonExistsByIDAsync(int personID)
        {
            bool exists = false;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_CheckPersonExistsByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PersonID", personID);

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
                        clsEventLogger.LogEvent($"SQL Error in PersonExistsByIDAsync (ID: {personID}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in PersonExistsByIDAsync (ID: {personID}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return exists;
        }


        /// <summary>
        /// Asynchronously checks if a person exists by NationalNo using SP_CheckPersonExistsByNationalNo.
        /// This method IS fully asynchronous (returns Task<bool>).
        /// </summary>
        /// <returns>A Task returning true if the person exists; otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown on database errors if SP uses THROW.</exception>
        /// <exception cref="Exception">Thrown on general errors.</exception>
        public static async Task<bool> PersonExistsByNationalNoAsync(string nationalNo)
        {
            bool exists = false;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_CheckPersonExistsByNationalNo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@NationalNo", nationalNo);

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
                        clsEventLogger.LogEvent($"SQL Error in PersonExistsByNationalNoAsync (NatNo: {nationalNo}): {ex.Message} (Number: {ex.Number})", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        clsEventLogger.LogEvent($"General Error in PersonExistsByNationalNoAsync (NatNo: {nationalNo}): {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                        throw; // Re-throw
                    }
                }
            }
            return exists;
        }
    }
}