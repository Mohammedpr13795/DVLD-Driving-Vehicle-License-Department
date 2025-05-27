using System;
using System.Data;
using System.Threading.Tasks;
using DVLD_DataAccess; // Reference DAL

// Assuming clsCountry exists, potentially in DVLD_Buisness or another referenced namespace
// using DVLD_Buisness; // If clsCountry is here
// using DVLD_DataAccess; // If clsDataAccessSettings is here (likely)

namespace DVLD_Buisness
{
    /// <summary>
    /// Represents a Person entity and handles business logic.
    /// Find methods are AASYNCHRONOUSLYLY, calling AASYNCHRONOUSLYLY DAL methods with ref parameters.
    /// Save, GetAll, Delete, Exists methods are AAASYNCHRONOUSLYLY, calling async DAL methods.
    /// Compatible with C# 7.3.
    /// </summary>
    public class clsPerson
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode { get; private set; } = enMode.AddNew;

        // Properties remain the same as your original code
        public int PersonID { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {SecondName} {ThirdName} {LastName}".Replace("  ", " ").Trim();

        public string NationalNo { get; set; }
        public DateTime DateOfBirth { get; set; }
        public short Gendor { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int NationalityCountryID { get; set; }
        public clsCountry CountryInfo { get; private set; } // Assuming clsCountry exists

        private string _ImagePath;
        public string ImagePath
        {
            get { return _ImagePath; }
            set { _ImagePath = value; }
        }

        /// <summary>
        /// Default constructor for AddNew mode.
        /// </summary>
        public clsPerson()
        {
            this.PersonID = -1;
            this.FirstName = "";
            this.SecondName = "";
            this.ThirdName = null;
            this.LastName = "";
            this.NationalNo = "";
            this.DateOfBirth = DateTime.MinValue;
            this.Gendor = 0;
            this.Address = "";
            this.Phone = "";
            this.Email = null;
            this.NationalityCountryID = -1;
            this.ImagePath = null;
            this.CountryInfo = null;
            Mode = enMode.AddNew;
        }

        /// <summary>
        /// Private constructor for loading existing data (Update mode).
        /// </summary>
        private clsPerson(int personID, string firstName, string secondName, string thirdName,
            string lastName, string nationalNo, DateTime dateOfBirth, short gendor,
             string address, string phone, string email,
            int nationalityCountryID, string imagePath , clsCountry CountryInfo)
        {
            this.PersonID = personID;
            this.FirstName = firstName;
            this.SecondName = secondName;
            this.ThirdName = thirdName;
            this.LastName = lastName;
            this.NationalNo = nationalNo;
            this.DateOfBirth = dateOfBirth;
            this.Gendor = gendor;
            this.Address = address;
            this.Phone = phone;
            this.Email = email;
            this.NationalityCountryID = nationalityCountryID;
            this.ImagePath = imagePath;
            this.CountryInfo =  CountryInfo; // Assumes clsCountry.Find exists
            Mode = enMode.Update;
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to add data via DAL. Called by SaveAsync.
        /// </summary>
        private async Task<bool> _AddNewPersonAsync()
        {
            // Calls the AAASYNCHRONOUSLYLY AddNewPersonAsync in DAL
            this.PersonID = await clsPersonData.AddNewPersonAsync(
                this.FirstName, this.SecondName, this.ThirdName,
                this.LastName, this.NationalNo,
                this.DateOfBirth, this.Gendor, this.Address, this.Phone, this.Email,
                this.NationalityCountryID, this.ImagePath);
            return (this.PersonID != -1);
        }

        /// <summary>
        /// Private AAASYNCHRONOUSLYLY method to update data via DAL. Called by SaveAsync.
        /// </summary>
        private async Task<bool> _UpdatePersonAsync()
        {
            // Calls the AAASYNCHRONOUSLYLY UpdatePersonAsync in DAL
            return await clsPersonData.UpdatePersonAsync(
                this.PersonID, this.FirstName, this.SecondName, this.ThirdName,
                this.LastName, this.NationalNo, this.DateOfBirth, this.Gendor,
                this.Address, this.Phone, this.Email,
                this.NationalityCountryID, this.ImagePath);
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a person by PersonID. Calls the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="PersonIDParam">The ID to search for.</param>
        /// <returns>A clsPerson object if found; otherwise, null.</returns>
        public static async Task <clsPerson> FindAsync(int PersonIDParam) // AASYNCHRONOUSLYLY signature
        {
            // Declare variables for ref parameters
            string FirstName = "", SecondName = "", ThirdName = null, LastName = "", NationalNo = "", Email = null, Phone = "", Address = "", ImagePath = null;
            DateTime DateOfBirth = DateTime.MinValue;
            int NationalityCountryID = -1;
            short Gendor = 0;
            bool IsFound = false;
            (IsFound, FirstName, SecondName,
          ThirdName, LastName, NationalNo, DateOfBirth,
           Gendor, Address, Phone, Email,
           NationalityCountryID, ImagePath) = await clsPersonData.GetPersonInfoByIDAsync(PersonIDParam);
            // Calls the AASYNCHRONOUSLYLY GetPersonInfoByID in DAL


            if (IsFound)
            {
                clsCountry CountryInfo = await clsCountry.FindAsync(NationalityCountryID); // Assumes clsCountry.Find exists
                return new clsPerson(PersonIDParam, FirstName, SecondName, ThirdName, LastName,
                          NationalNo, DateOfBirth, Gendor, Address, Phone, Email, NationalityCountryID, ImagePath , CountryInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AASYNCHRONOUSLYLYLY finds a person by National Number. Calls the AASYNCHRONOUSLYLY DAL method.
        /// </summary>
        /// <param name="NationalNoParam">The National Number to search for.</param>
        /// <returns>A clsPerson object if found; otherwise, null.</returns>
        public static async Task<clsPerson> FindAsync(string NationalNoParam) // AASYNCHRONOUSLYLY signature
        {
            // Declare variables for ref parameters
            string FirstName = "", SecondName = "", ThirdName = null, LastName = "", Email = null, Phone = "", Address = "", ImagePath = "";
            DateTime DateOfBirth = DateTime.MinValue;
            int PersonID = -1, NationalityCountryID = -1;
            short Gendor = 0;
            bool IsFound = false;
            // Calls the AASYNCHRONOUSLYLY GetPersonInfoByNationalNo in DAL
            (IsFound, PersonID, FirstName, SecondName,
            ThirdName, LastName, DateOfBirth,
             Gendor, Address, Phone, Email,
             NationalityCountryID, ImagePath) = await clsPersonData.GetPersonInfoByNationalNoAsync(NationalNoParam);

            if (IsFound)
            {
                clsCountry CountryInfo = await clsCountry.FindAsync(NationalityCountryID); // Assumes clsCountry.Find exists

                return new clsPerson(PersonID, FirstName, SecondName, ThirdName, LastName,
                          NationalNoParam, DateOfBirth, Gendor, Address, Phone, Email, NationalityCountryID, ImagePath , CountryInfo);
            }
            else
                return null;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY saves the current person object (Adds New or Updates).
        /// </summary>
        /// <returns>A Task returning true if the save operation was successful; false otherwise.</returns>
        public async Task<bool> SaveAsync() // Async method
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    // Await the ASYNC private add method
                    if (await _AddNewPersonAsync())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else { return false; }

                case enMode.Update:
                    // Await the ASYNC private update method
                    return await _UpdatePersonAsync();
            }
            return false;
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY gets a DataTable containing all people.
        /// </summary>
        /// <returns>A Task returning a DataTable with people information.</returns>
        public static async Task<DataTable> GetAllPeopleAsync() // Async method
        {
            // Call ASYNC DAL method
            return await clsPersonData.GetAllPeopleAsync();
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY deletes a person by their ID.
        /// </summary>
        /// <param name="personID">The ID of the person to delete.</param>
        /// <returns>A Task returning true if deletion was successful; false otherwise.</returns>
        public static async Task<bool> DeletePersonAsync(int personID) // Async method
        {
            if (personID <= 0) return false;
            // Call ASYNC DAL method
            return await clsPersonData.DeletePersonAsync(personID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if a person exists by their ID.
        /// </summary>
        /// <param name="personID">The Person ID to check.</param>
        /// <returns>A Task returning true if the person exists; false otherwise.</returns>
        public static async Task<bool> PersonExistsAsync(int personID) // Async method
        {
            if (personID <= 0) return false;
            // Call ASYNC DAL method
            return await clsPersonData.PersonExistsByIDAsync(personID);
        }

        /// <summary>
        /// AAASYNCHRONOUSLYLYLY checks if a person exists by their National Number.
        /// </summary>
        /// <param name="nationalNo">The National Number to check.</param>
        /// <returns>A Task returning true if the person exists; false otherwise.</returns>
        public static async Task<bool> PersonExistsAsync(string nationalNo) // Async method
        {
            if (string.IsNullOrWhiteSpace(nationalNo)) return false;
            // Call ASYNC DAL method
            return await clsPersonData.PersonExistsByNationalNoAsync(nationalNo);
        }


    }
}