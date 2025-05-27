using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DVLD.Global_Classes;
using DVLD_Buisness;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace DVLD.Classes
{
    internal static class clsGlobal
    {
        public static clsUser CurrentUser;

        public class clsCredentialResult
        {
            public bool Success { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        #region Files
        public static async Task<bool> RememberUsernameAndPassword(string Username, string Password)
        {

            try
            {
                //this will get the current project directory folder.
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();


                // Define the path to the text file where you want to save the data
                string filePath = currentDirectory + "\\data.txt";

                //incase the username is empty, delete the file
                if (Username == "" && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;

                }

                // concatonate username and passwrod withe seperator.
                string dataToSave = Username + "#//#" + Password;

                // Create a StreamWriter to write to the file
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write the data to the file
                    writer.WriteLine(dataToSave);

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }

        }

        public static async Task<clsCredentialResult> GetStoredCredential()
        {
            //this will get the stored username and password and will return true if found and false if not found.
            try
            {
                //gets the current project's directory
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();

                // Path for the file that contains the credential.
                string filePath = currentDirectory + "\\data.txt";
                string Username = null , Password = null;
                // Check if the file exists before attempting to read it
                if (File.Exists(filePath))
                {
                    // Create a StreamReader to read from the file
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        // Read data line by line until the end of the file
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Console.WriteLine(line); // Output each line of data to the console
                            string[] result = line.Split(new string[] { "#//#" }, StringSplitOptions.None);

                            Username = result[0];
                            Password = result[1];

                        }
                        return new clsCredentialResult { Success = false, Username = Username, Password = Password };

                    }

                }
                else
                {
                    return new clsCredentialResult { Success = false, Username = null, Password = null };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return new clsCredentialResult { Success = false, Username = null, Password = null };
            }

        }

        #endregion

        #region Windows Registry


        public static async Task UpdatePasswordAsync(clsUser User , string Password)
        {
            if (User.IsRemember)
            {
                await clsGlobal.SaveCredentialsToRegistry(User.UserName.Trim(), Password.Trim());
                //frmLogin.UserNameAndPassword[0] = frmMain.User.UserName;
                //frmLogin.UserNameAndPassword[1] = frmMain.User.Password;
                //File.WriteAllLines(frmLogin.FilePath, frmLogin.UserNameAndPassword);
            }
        }

        /// <summary>
        /// Retrieves the username and password from the Windows Registry under HKEY_CURRENT_USER\SOFTWARE\DVLD.
        /// </summary>
        /// <param name="UserName">The retrieved username will be stored here.</param>
        /// <param name="Password">The retrieved password will be stored here.</param>
        /// <returns>True if the credentials are found; otherwise, false.</returns>
        public static async Task<bool> SaveCredentialsToRegistry(string username, string password)
        {
            // تحقق من القيم الفارغة
            if (string.IsNullOrEmpty(username))
            {
                try
                {
                    // فتح المفتاح الأساسي بعرض 64-بت
                    using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                    {
                        // فتح المفتاح الفرعي بأذونات الكتابة
                        using (RegistryKey subKey = baseKey.OpenSubKey(@"SOFTWARE\DVLD_App", true))
                        {
                            if (subKey != null)
                            {
                                subKey.DeleteValue("UserName", throwOnMissingValue: false);
                                subKey.DeleteValue("Password", throwOnMissingValue: false);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to clear credentials: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                    return false;
                }
            }

            // تحديد مسار المفتاح في السجل
            string keyPath = @"SOFTWARE\DVLD_App";
            const string usernameKey = "UserName";
            const string passwordKey = "Password";

            try
            {
                // فتح المفتاح الأساسي بعرض 64-بت
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                {
                    // إنشاء أو فتح المفتاح الفرعي بأذونات الكتابة
                    using (RegistryKey subKey = baseKey.CreateSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        if (subKey != null)
                        {
                            subKey.SetValue(usernameKey, username, RegistryValueKind.String);
                            subKey.SetValue(passwordKey, password, RegistryValueKind.String);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Failed to create registry key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Retrieves the username and password from the Windows Registry under HKEY_CURRENT_USER\SOFTWARE\DVLD.
        /// </summary>
        /// <param name="UserName">The retrieved username will be stored here.</param>
        /// <param name="Password">The retrieved password will be stored here.</param>
        /// <returns>True if the credentials are found; otherwise, false.</returns>
        public static async Task<clsCredentialResult> GetCredentialsFromRegistry()
        {
            try
            {
                // تحديد مسار المفتاح في السجل
                string keyPath = @"SOFTWARE\DVLD_App";
                const string usernameKey = "UserName";
                const string passwordKey = "Password";

                // فتح المفتاح الأساسي بعرض 64-بت
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                {
                    // فتح المفتاح الفرعي للقراءة
                    using (RegistryKey subKey = baseKey.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if (subKey != null)
                        {
                            // استرجاع القيم
                            string usernameValue = subKey.GetValue(usernameKey) as string;
                            string passwordValue = subKey.GetValue(passwordKey) as string;

                            // التحقق مما إذا كانت القيم موجودة
                            if (usernameValue != null && passwordValue != null)
                            {
                                //UserName = usernameValue;
                                //Password = passwordValue;
                                return new clsCredentialResult { Success = true, Username = usernameValue, Password = passwordValue};
                            }
                            else
                            {
                                return new clsCredentialResult { Success = false, Username = null, Password = null};
                            }
                        }
                        else
                        {
                            return new clsCredentialResult { Success = false, Username = null, Password = null};

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return new clsCredentialResult { Success = false, Username = null, Password = null };
            }
        }
        #endregion



    }
}