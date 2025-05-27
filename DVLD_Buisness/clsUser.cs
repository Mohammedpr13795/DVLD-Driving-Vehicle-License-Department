using System;
using System.Data;
using System.Threading.Tasks; // Required for async methods
using DVLD_DataAccess; // Reference DAL

// Assuming clsPerson and clsCountry are accessible
// Assuming clsCryptoHelper exists and provides ComputeHashPassword method

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a User entity and handles business logic.
    /// Find methods are AASYNCHRONOUSLYLY to match original design.
    /// Save, GetAll, Delete, Exists methods are AAASYNCHRONOUSLYLY.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsUser
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        public int UserID { get; set; }
        public int PersonID { get; set; }
        public clsPerson PersonInfo { get; private set; } // Loaded AASYNCHRONOUSLYLYly in Find/Constructor
        public string UserName { get; set; }
        public string Password { get; set; } // Stores the HASH after Find or during Add/Update preparation
        public bool IsActive { get; set; }

        // IsRemember is UI-specific state, usually not part of the core User business object
         public bool IsRemember { set; get; }

        /// <summary>
        /// Constructor for creating a new user (AddNew mode).
        /// Password should be set before calling SaveAsync.
        /// </summary>
        public clsUser()
        {
            this.UserID = -1;
            this.PersonID = -1; // Indicate no person linked yet
            this.PersonInfo = null;
            this.UserName = "";
            this.Password = ""; // Initially empty, will be hashed before saving
            this.IsActive = true;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing user data (Update mode).
        /// Called by Find methods. Assumes password parameter is the HASH from DB.
        /// </summary>
        private clsUser(int userID, int personID, string username, string passwordHash, bool isActive , clsPerson PersonInfo)
        {
            this.UserID = userID;
            this.PersonID = personID;
            this.UserName = username;
            this.Password = passwordHash; // Store the hash retrieved from DB
            this.IsActive = isActive;
            // Load PersonInfo AASYNCHRONOUSLYLYly using the already AASYNCHRONOUSLYLY clsPerson.Find
            this.PersonInfo = PersonInfo;
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add the new user data via DAL.
        /// Hashes the current Password property before sending to DAL.
        /// </summary>
        private async Task<bool> _AddNewUserAsync()
        {
            // Hash the password before sending it to DAL
            string hashedPassword = clsCryptoHelper.ComputeHashPassword(this.Password);

            // Call the AAASYNCHRONOUSLYLY DAL method
            this.UserID = await clsUserData.AddNewUserAsync(this.PersonID, this.UserName,
                hashedPassword, this.IsActive);

            // After adding, update the Password property to store the hash
            if (this.UserID != -1)
            {
                this.Password = hashedPassword; // Store the hash in the object after successful add
            }

            return (this.UserID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update the existing user data via DAL.
        /// Hashes the current Password property before sending to DAL.
        /// </summary>
        private async Task<bool> _UpdateUserAsync()
        {
            // Hash the password before sending it to DAL (in case it was changed)
            string hashedPassword = clsCryptoHelper.ComputeHashPassword(this.Password);

            // Call the AAASYNCHRONOUSLYLY DAL method
            bool success = await clsUserData.UpdateUserAsync(this.UserID, this.PersonID, this.UserName,
                hashedPassword, this.IsActive);

            // If update was successful, ensure the object's Password property holds the latest hash
            if (success)
            {
                this.Password = hashedPassword;
            }
            return success;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a user by UserID. Calls the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <returns>A clsUser object if found; otherwise, null.</returns>
        public static async Task<clsUser> FindByUserIDAsync(int UserID) // Sync signature
        {
            int PersonID = -1;
            string UserName = "", PasswordHash = ""; // Expecting hash from DAL
            bool IsFound = false, IsActive = false;

            // Call AASYNCHRONOUSLYLY DAL method
            (IsFound , PersonID , UserName , PasswordHash , IsActive)= await clsUserData.GetUserInfoByUserIDAsync(UserID);

            if (IsFound)
            {
                clsPerson PersonInfo = await clsPerson.FindAsync(PersonID);
                // Pass the HASH to the constructor
                return new clsUser(UserID, PersonID, UserName, PasswordHash, IsActive , PersonInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a user by PersonID. Calls the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <returns>A clsUser object if found; otherwise, null.</returns>
        public static async Task<clsUser> FindByPersonIDAsync(int PersonID) // Sync signature
        {
            int UserID = -1;
            string UserName = "", PasswordHash = ""; // Expecting hash from DAL
            bool IsFound  , IsActive = false;

            // Call AASYNCHRONOUSLYLY DAL method
            (IsFound , UserID , UserName , PasswordHash , IsActive)= await clsUserData.GetUserInfoByPersonIDAsync(PersonID);

            if (IsFound)
            {
                clsPerson PersonInfo = await clsPerson.FindAsync(PersonID);
                // Pass the HASH to the constructor
                // Original code had a bug here: return new clsUser(UserID, UserID, ...); should be PersonID
                return new clsUser(UserID, PersonID, UserName, PasswordHash, IsActive , PersonInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a user by username and PLAIN TEXT password.
        /// Hashes the password before calling the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="PasswordPlainText">The plain text password entered by the user.</param>
        /// <returns>A clsUser object if found and credentials match; otherwise, null.</returns>
        public static async Task<clsUser> FindByUsernameAndPasswordAsync(string UserName, string PasswordPlainText) // Sync signature
        {
            int UserID = -1, PersonID = -1;
            bool IsFound = false, IsActive = false;

            // Hash the provided plain text password
            string hashedPassword = clsCryptoHelper.ComputeHashPassword(PasswordPlainText);

            // Call the AASYNCHRONOUSLYLY DAL method that expects the HASH
             (IsFound,UserID , PersonID , IsActive)= await clsUserData.GetUserInfoByUsernameAndPasswordAsync(UserName, hashedPassword);

            if (IsFound)
            {
                clsPerson PersonInfo = await clsPerson.FindAsync(PersonID);
                // IMPORTANT: Pass the HASHED password to the constructor, not the plain text one.
                // The Password property of clsUser will store the hash.
                return new clsUser(UserID, PersonID, UserName, hashedPassword, IsActive, PersonInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current user object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save operation was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async method
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    // Await the ASYNC private add method
                    if (await _AddNewUserAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    // Await the ASYNC private update method
                    return await _UpdateUserAsync();
            }
            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all users.
        /// </summary>
        /// <returns>A Task returning a DataTable with user information.</returns>
        public static async Task<DataTable> GetAllUsersAsync() // Async method
        {
            // Call ASYNC DAL method
            return await clsUserData.GetAllUsersAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY deletes a user by their ID.
        /// </summary>
        /// <param name="UserID">The ID of the user to delete.</param>
        /// <returns>A Task returning true if deletion was successful; false otherwise.</returns>
        public static async Task<bool> DeleteUserAsync(int UserID) // Async method
        {
            if (UserID <= 0) return false;
            // Call ASYNC DAL method
            return await clsUserData.DeleteUserAsync(UserID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if a user exists by their UserID.
        /// </summary>
        /// <param name="UserID">The User ID to check.</param>
        /// <returns>A Task returning true if the user exists; false otherwise.</returns>
        public static async Task<bool> UserExistsAsync(int UserID) // Async method, renamed
        {
            if (UserID <= 0) return false;
            // Call ASYNC DAL method
            return await clsUserData.UserExistsByIDAsync(UserID); // Call renamed DAL method
        }


        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if a user exists by their Username.
        /// </summary>
        /// <param name="UserName">The Username to check.</param>
        /// <returns>A Task returning true if the user exists; false otherwise.</returns>
        public static async Task<bool> UserExistsAsync(string UserName) // Async method, renamed
        {
            if (string.IsNullOrWhiteSpace(UserName)) return false;
            // Call ASYNC DAL method
            return await clsUserData.UserExistsByUsernameAsync(UserName); // Call renamed DAL method
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if a user exists for a specific PersonID.
        /// </summary>
        /// <param name="PersonID">The Person ID to check.</param>
        /// <returns>A Task returning true if a user is linked to the PersonID; false otherwise.</returns>
        public static async Task<bool> UserExistsForPersonIDAsync(int PersonID) // Async method, renamed
        {
            if (PersonID <= 0) return false;
            // Call ASYNC DAL method
            return await clsUserData.UserExistsByPersonIDAsync(PersonID); // Call renamed DAL method
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY changes the password for the current user instance.
        /// Hashes the new password before calling the DAL.
        /// </summary>
        /// <param name="NewPasswordPlainText">The new plain text password.</param>
        /// <returns>A Task returning true if the password change was successful, false otherwise.</returns>
        public async Task<bool> ChangePasswordAsync(string NewPasswordPlainText) // Instance method, Async
        {
            if (this.UserID <= 0) return false; // Ensure user exists (has an ID)

            // Hash the new plain text password
            string newHashedPassword = clsCryptoHelper.ComputeHashPassword(NewPasswordPlainText);

            // Call the ASYNC DAL method to change the password in the DB
            bool success = await clsUserData.ChangePasswordAsync(this.UserID, newHashedPassword);

            // If successful, update the hash stored in this object instance
            if (success)
            {
                this.Password = newHashedPassword;
            }
            return success;
        }

        // --- Keep original AASYNCHRONOUSLYLY static methods if absolutely necessary for backward compatibility ---
        // --- but strongly recommend updating calling code (UI) to use Async versions ---
        /*
        public static DataTable GetAllUsers() => GetAllUsersAsync().GetAwaiter().GetResult();
        public static bool DeleteUser(int UserID) => DeleteUserAsync(UserID).GetAwaiter().GetResult();
        public static bool isUserExist(int UserID) => UserExistsAsync(UserID).GetAwaiter().GetResult();
        public static bool isUserExist(string UserName) => UserExistsAsync(UserName).GetAwaiter().GetResult();
        public static bool isUserExistForPersonID(int PersonID) => UserExistsForPersonIDAsync(PersonID).GetAwaiter().GetResult();
        */

    } // End Class clsUser
} // End Namespace