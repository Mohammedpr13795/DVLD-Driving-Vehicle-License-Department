using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DVLD.Classes;
using DVLD.Properties;
using DVLD_Buisness;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Runtime.ConstrainedExecution;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Eventing.Reader;
using DVLD.Global_Classes;

namespace DVLD.People
{
    public partial class frmAddUpdatePerson : Form
    {

        // Declare a delegate
        //public delegate void DataBackEventHandler(object sender, int PersonID);


        //Use Build In Delegate
        public event Action<object, int> DataBack;

        // Declare an event using the delegate
        //public event DataBackEventHandler DataBack;

        public enum enMode { AddNew = 0, Update = 1 };

        private enMode _Mode;
        private int _PersonID = -1;
        clsPerson _Person;

        // New Proprerty From My Project
        bool _ISRequiredFieldsUnchanged;
        bool _IsOptionalFieldsUnchanged;
        enGendor Gendor = enGendor.Male;
        // New Proprerty From My Project
        public enum enGendor { Male = 0, Female = 1 };


        public frmAddUpdatePerson()
        {
            InitializeComponent();
            _Mode = enMode.AddNew;

        }

        public frmAddUpdatePerson(int PersonID)
        {
            InitializeComponent();

            _Mode = enMode.Update;
            _PersonID = PersonID;
        }

        ///////////////////////////// ///////////////////////////// /////////////////////////////
        ///////////////////////////// New Method From My Projec /////////////////////////////
        ///////////////////////////// ///////////////////////////// /////////////////////////////
 

        private bool _CheckIsNullOrEmpty(TextBox txt, CancelEventArgs e)
        {

            if (string.IsNullOrEmpty(txt.Text.Trim()))
            {
                rbMale.Enabled = false;
                rbFemale.Enabled = false;
                //_IsUsed = true;
                e.Cancel = true;
                // txt.Focus();
                erproValidting.SetError(txt, " This field is required!");
                return true;
            }
            return false;
        }

        private bool _IsFirstLetterUpper(TextBox txt, CancelEventArgs e)
        {
            // تحقق من أن النص يبدأ بحرف كبير (إنجليزي أو عربي)
            if (!clsValidatoin.IsFirstLetterUpper(txt.Text.Trim()))
            {
                rbFemale.Enabled = false;
                rbFemale.Enabled = false;
                //_IsUsed = false;
                //_Error = true;
                e.Cancel = true;
                txt.Focus();
                erproValidting.SetError(txt, "The First Letter Must Be Uppercase!");
                return false;
            }
            return true;

        }
        private bool _AreRemainingLettersLowercase(TextBox txt, CancelEventArgs e)
        {
            // تحقق من أن الحروف بعد الحرف الأول كلها صغيرة (إنجليزي أو عربي)
            if (!clsValidatoin.AreRemainingLettersLowercase(txt.Text.Trim()))
            {
                rbMale.Enabled = false;
                rbFemale.Enabled = false;
                //_Error = true;
                e.Cancel = true;
                txt.Focus();
                erproValidting.SetError(txt, "All Letters After The First One Must Be Lowercase.");
                return false;
            }

            return true;
        }

        private bool _ValidatePhoneNumber(TextBox txt, CancelEventArgs e)
        {
  
            if (!clsValidatoin.IsTenDigitsOnly(txt.Text))
            {
                //_Error = true;
                e.Cancel = true;
                txt.Focus();
                erproValidting.SetError(txt, "Please enter exactly 10 digits.");
                return false;
            }

            return true;
        }

        private void _ClearValidationError(TextBox txt, CancelEventArgs e)
        {
            //if (_Error) _Error = false;
            //else _IsUsed = false;

            rbMale.Enabled = true;
            rbFemale.Enabled = true;
            e.Cancel = false;
            erproValidting.SetError(txt, null);
        }
        private bool _IsDataUnchanged()
        {

            // التحقق من الحقول الاختيارية

            if (_Mode == enMode.AddNew) return false;

            bool IsThirdNameUnchanged = false;
            if (!(_Person.ThirdName == null))
            { if (_Person.ThirdName.Trim() == txtThirdName.Text.Trim()) 
                    IsThirdNameUnchanged = true; 
                else IsThirdNameUnchanged = false; }

            else if (string.IsNullOrEmpty(txtThirdName.Text)) IsThirdNameUnchanged = true;

            else IsThirdNameUnchanged = false;

            bool IsEmailUnchanged = false;
            if (!(_Person.Email == null)) 
            { if (_Person.Email.Trim() == txtEmail.Text.Trim()) IsEmailUnchanged = true;
                else IsEmailUnchanged = false; }

            else if (string.IsNullOrEmpty(txtEmail.Text.Trim())) IsEmailUnchanged = true;

            else IsEmailUnchanged = false;

            bool IsImageUnchanged = false;
            if (!(_Person.ImagePath == null))
            {

                if (string.IsNullOrEmpty(pbPersonImage.ImageLocation)) IsImageUnchanged = false;

                else if (_Person.ImagePath == pbPersonImage.ImageLocation) IsImageUnchanged = true;

                //else if (_Person.ImagePath == PathImageAfterSaveOrBeforDelete) IsImageUnchanged = true; 

                else IsImageUnchanged = false;

            }


            else if (pbPersonImage.ImageLocation == null) IsImageUnchanged = true;

            else IsImageUnchanged = false;


            _IsOptionalFieldsUnchanged = IsThirdNameUnchanged && IsEmailUnchanged && IsImageUnchanged;



            // التحقق من الحقول المطلوبة
            _ISRequiredFieldsUnchanged = _Person.FirstName.Trim() == txtFirstName.Text.Trim()
                                        && _Person.SecondName.Trim() == txtSecondName.Text.Trim()
                                        && _Person.LastName.Trim() == txtLastName.Text.Trim()
                                        && _Person.NationalNo.Trim() == txtNationalNo.Text.Trim()
                                        && _Person.DateOfBirth == dtpDateOfBirth.Value
                                        && _Person.Gendor == (byte)Gendor
                                        && _Person.Phone.Trim() == txtPhone.Text.Trim()
                                        && _Person.NationalityCountryID == cbCountry.SelectedIndex + 1
                                        && _Person.Address.Trim() == txtAddress.Text.Trim()
                                        ;

            return _IsOptionalFieldsUnchanged && _ISRequiredFieldsUnchanged;
        }



        ///////////////////////////// ///////////////////////////// /////////////////////////////
        ///////////////////////////// New Method From My Projec /////////////////////////////
        ///////////////////////////// ///////////////////////////// /////////////////////////////
        private async Task _ResetDefualtValues()
        {
            //this will initialize the reset the defaule values
            await _FillCountriesInComoboBox();

            if (_Mode == enMode.AddNew)
            {
                lblTitle.Text = "Add New Person";
                _Person = new clsPerson();
            } 
            else
            {
                lblTitle.Text = "Update Person";
            }

            //set default image for the person.
            pbPersonImage.Image = rbMale.Checked ? Resources.Male_512 : Resources.Female_512;

            //hide/show the remove linke incase there is no image for the person.
            llRemoveImage.Visible = (pbPersonImage.ImageLocation != null);

            //we set the max date to 18 years from today, and set the default value the same.
            dtpDateOfBirth.MaxDate = DateTime.Now.AddYears(-18);
            dtpDateOfBirth.Value = dtpDateOfBirth.MaxDate;

            //should not allow adding age more than 100 years
            dtpDateOfBirth.MinDate = DateTime.Now.AddYears(-100);

            //this will set default country to jordan.
            cbCountry.SelectedIndex = cbCountry.FindString("Saudi Arabia");
            //cbCountry.SelectedIndex = cbCountry.SelectedIndex  - 1;

            txtFirstName.Text = "";
            txtSecondName.Text = "";
            txtThirdName.Text = "";
            txtLastName.Text = "";
            //txtNationalNo.Text = "";
            rbMale.Checked = true;
            //txtPhone.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";


        }

        private async Task _FillCountriesInComoboBox()
        {
            DataTable dtCountries = await clsCountry.GetAllCountriesAsync();

            foreach (DataRow row in dtCountries.Rows)
            {
                cbCountry.Items.Add(row["CountryName"]);
            }
        }

        private async Task _LoadData()
        {
           
            _Person = await clsPerson.FindAsync(_PersonID);

            if (_Person == null)
            {
                MessageBox.Show("No Person with ID = " + _PersonID ,"Person Not Found",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                this.Close();
                return;
            }

            //the following code will not be executed if the person was not found
            lblPersonID.Text = _PersonID.ToString();
            txtFirstName.Text = _Person.FirstName;
            txtSecondName.Text = _Person.SecondName;
            txtThirdName.Text = _Person.ThirdName;
            txtLastName.Text = _Person.LastName;
            txtNationalNo.Text = _Person.NationalNo;
            dtpDateOfBirth.Value = _Person.DateOfBirth;

            if ((enGendor)_Person.Gendor == enGendor.Male)
            { rbMale.Checked = true; Gendor = enGendor.Male; }
            else
            { rbFemale.Checked = true; Gendor = enGendor.Female; }

            txtAddress.Text = _Person.Address;
            txtPhone.Text = _Person.Phone;
            txtEmail.Text = _Person.Email;
            //cbCountry.SelectedIndex = cbCountry.FindString(_Person.CountryInfo.CountryName);
            cbCountry.SelectedIndex = _Person.NationalityCountryID - 1;

            //load person image incase it was set.
            if (_Person.ImagePath != "")
                pbPersonImage.ImageLocation = _Person.ImagePath;

            else
                pbPersonImage.Image = (enGendor)_Person.Gendor == enGendor.Male ? Resources.Male_512 : Resources.Female_512;
            //    if (_Person.Gendor == 0)
            //    pbPersonImage.Image = Properties.Resources.Male_512;

            //else
            //    pbPersonImage.Image = Properties.Resources.Male_512;


            //hide/show the remove linke incase there is no image for the person.
            llRemoveImage.Visible = (_Person.ImagePath != ""); 

        }

        private async void frmAddUpdatePerson_Load(object sender, EventArgs e)
        {
            await _ResetDefualtValues();

            if(_Mode==enMode.Update)
                await _LoadData();
        }

        private async Task<bool> _HandlePersonImage()
        {

            //this procedure will handle the person image,
            //it will take care of deleting the old image from the folder
            //in case the image changed. and it will rename the new image with guid and 
            // place it in the images folder.

          
                //_Person.ImagePath contains the old Image, we check if it changed then we copy the new image
                if (_Person.ImagePath != pbPersonImage.ImageLocation)
                {
                    if (!string.IsNullOrEmpty(_Person.ImagePath))
                    {
                    //first we delete the old image from the folder in case there is any.

                        try
                        {
                            File.Delete(_Person.ImagePath);
                        }
                        catch (IOException ex)
                        {
                        await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                        // We could not delete the file.
                        //log it later   
                    }
                }

                    if (pbPersonImage.ImageLocation != null)
                    {
                        //then we copy the new image to the image folder after we rename it
                        string SourceImageFile =pbPersonImage.ImageLocation.ToString();

                    (bool Success, string NewFilePath) = await clsUtil.CopyImageToProjectImagesFolderAsync(SourceImageFile);
                    if (Success)
                        {
                            pbPersonImage.ImageLocation = NewFilePath;
                             return true;
                        }
                        else
                        {
                            MessageBox.Show("Error Copying Image File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }

            }
            return true;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {

            if (!this.ValidateChildren())
            {
                //Here we dont continue becuase the form is not valid
                MessageBox.Show("Some fileds are not valide!, put the mouse over the red icon(s) to see the erro","Validation Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;

            }

            if (_Mode == enMode.Update)
            {
                if (_IsDataUnchanged())
                {
                    MessageBox.Show("No changes were detected in the information. Save operation was not performed.", "No Changes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            if (!await _HandlePersonImage())
                return;

            int NationalityCountryID = await clsCountry.FindCountryIDByCountryNameAsync(cbCountry.Text);

            _Person.FirstName = txtFirstName.Text.Trim();
            _Person.SecondName = txtSecondName.Text.Trim();
            _Person.ThirdName = txtThirdName.Text.Trim();
            _Person.LastName = txtLastName.Text.Trim();
            _Person.NationalNo = txtNationalNo.Text.Trim() ;
            _Person.Email = txtEmail.Text.Trim();
            _Person.Phone = txtPhone.Text.Trim();
            _Person.Address = txtAddress.Text.Trim();
            _Person.DateOfBirth = dtpDateOfBirth.Value;

            if (rbMale.Checked)
                _Person.Gendor = (short) enGendor.Male;
            else
                _Person.Gendor = (short) enGendor.Female;

            _Person.NationalityCountryID = NationalityCountryID;
            
            if (pbPersonImage.ImageLocation != null)
                _Person.ImagePath = pbPersonImage.ImageLocation;
            else
                _Person.ImagePath = "";

            if (await _Person.SaveAsync())
            {
                 lblPersonID.Text = _Person.PersonID.ToString();
                //change form mode to update.
                _Mode = enMode.Update;
                lblTitle.Text = "Update Person";

                MessageBox.Show("Data Saved Successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

    
                // Trigger the event to send data back to the caller form.
                DataBack?.Invoke(this, _Person.PersonID);
            }
            else
                MessageBox.Show("Error: Data Is not Saved Successfully.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            
            
        }

        private void llSetImage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Process the selected file
                string selectedFilePath = openFileDialog1.FileName;
                pbPersonImage.Load(selectedFilePath);
                llRemoveImage.Visible = true;
                // ...
            }
        }

        private void llRemoveImage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           
            pbPersonImage.ImageLocation = null;


            pbPersonImage.Image = rbMale.Checked ? Resources.Male_512 : Resources.Female_512;
            //if (rbMale.Checked)
            //    pbPersonImage.Image = Resources.Male_512;
            //else
            //    pbPersonImage.Image = Resources.Female_512;

            llRemoveImage.Visible = false;
        }

        private void rbFemale_Click(object sender, EventArgs e)
        {
           //change the defualt image to female incase there is no image set.
            if (pbPersonImage.ImageLocation == null)
                pbPersonImage.Image = Resources.Female_512;
        }

        private void rbMale_Click(object sender, EventArgs e)
        {
            //change the defualt image to male incase there is no image set.
            if (pbPersonImage.ImageLocation == null)
                pbPersonImage.Image = Resources.Male_512;
        }

        private void ValidateTextBox(object sender, CancelEventArgs e)
        {


            // First: set AutoValidate property of your Form to EnableAllowFocusChange in designer 
            TextBox Temp = ((TextBox) sender);
            //if (Temp.Tag == txtThirdName.Tag && _CheckIsNullOrEmpty(Temp, e)) { _ClearValidationError(Temp, e); return; }
            
            if (_CheckIsNullOrEmpty(Temp, e)) return;
            else if (!_IsFirstLetterUpper(Temp, e)) return;

            else if (!_AreRemainingLettersLowercase(Temp, e)) return;


            else _ClearValidationError(Temp, e);



        }

        private void txtEmail_Validating(object sender, CancelEventArgs e)
        {
            //no need to validate the email incase it's empty.
            if (txtEmail.Text.Trim() == "")
                return;

            //validate email format
            if (!clsValidatoin.ValidateEmail(txtEmail.Text))
            {
                e.Cancel = true;
                erproValidting.SetError(txtEmail, "Invalid Email Address Format!");
            }
           else
            { 
                erproValidting.SetError(txtEmail, null); 
            };

        }

        private async void txtNationalNo_Validating(object sender, CancelEventArgs e)
        {

            
            //Make sure the national number is not used by another person
            if (txtNationalNo.Text.Trim().ToUpper() != _Person.NationalNo && await clsPerson.PersonExistsAsync(txtNationalNo.Text.Trim()))
            {
                //if (_Person.NationalNo == txtNationalNo.Text.Trim()) return;
                    e.Cancel = true;
                erproValidting.SetError(txtNationalNo, "National Number is used for another person!");
            
            }
            else
            {
                erproValidting.SetError(txtNationalNo, null);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // السماح فقط بالأرقام ومفاتيح التحكم مثل Backspace
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // منع تجاوز 10 أرقام
            if (char.IsDigit(e.KeyChar) && textBox.Text.Trim().Length >= 10)
            {
                e.Handled = true;
                return;
            }

            // منع حذف "05" في البداية
            if (char.IsControl(e.KeyChar) && textBox.SelectionStart <= 2 && textBox.Text.Trim().StartsWith("05"))
            {
                e.Handled = true;
            }
        }

        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar);
        }


        private void txtThirdName_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtThirdName.Text.Trim())) { _ClearValidationError(txtThirdName, e); return; }

            if (!_IsFirstLetterUpper(txtThirdName, e)) return;

            else if (!_AreRemainingLettersLowercase(txtThirdName, e)) return;


            else _ClearValidationError(txtThirdName, e);
        }

        private void txtAddress_Validating(object sender, CancelEventArgs e)
        {
            if (_CheckIsNullOrEmpty(txtAddress, e)) return;
            else _ClearValidationError(txtAddress, e);

        }

        private void txtNationalNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // التأكد من أن النص يبدأ دائمًا بحرف 'N'
            if (!textBox.Text.StartsWith("N"))
            {
                textBox.Text = "N";
                textBox.SelectionStart = textBox.Text.Length; // نقل المؤشر إلى نهاية النص
            }

            // السماح فقط بالأرقام بعد "N"
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                return;
            }


            // منع مسح "N"
            if (char.IsControl(e.KeyChar) && textBox.SelectionStart == 1 && textBox.Text == "N")
            {
                e.Handled = true;
            }
        }

        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPhone.Text) || !txtPhone.Text.StartsWith("05"))
            { txtPhone.Text = "05"; txtPhone.SelectionStart = txtPhone.Text.Length; }
            ;
        }

        private void txtNationalNo_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNationalNo.Text) || !txtNationalNo.Text.StartsWith("N"))
            { txtNationalNo.Text = "N"; txtNationalNo.SelectionStart = txtNationalNo.Text.Length; }
            ;
        }
        private bool _ValidateTextBoxes()
        {
            return string.IsNullOrEmpty(txtFirstName.Text) &&
                    string.IsNullOrEmpty(txtSecondName.Text) &&
                    string.IsNullOrEmpty(txtThirdName.Text) &&
                    string.IsNullOrEmpty(txtLastName.Text) &&
                    string.IsNullOrEmpty(txtAddress.Text);
        }

        private void frmAddUpdatePerson_FormClosing(object sender, FormClosingEventArgs e)
        {
            // التحقق من الوضع (Update أو AddNew)
            if (_Mode == enMode.Update)
            {
                // استخدام _IsDataUnchanged في وضع Update
                if (!_IsDataUnchanged())
                {
                    _HandleUnsavedChanges(e);
                }
                else
                    e.Cancel = false;

            }
            else if (_Mode == enMode.AddNew)
            {
                // استخدام _ValidateTextBoxes في وضع AddNew
                if (!_ValidateTextBoxes())
                {
                    _HandleUnsavedChanges(e);
                }
                else
                    e.Cancel = false;
            }
        }

        // دالة للتحقق من التغييرات غير المحفوظة وعرض رسالة تحذير
        private void _HandleUnsavedChanges(FormClosingEventArgs e)
        {
            // عرض رسالة تحذير للمستخدم
            DialogResult result = MessageBox.Show(
                "You have unsaved data. Are you sure you want to close without saving?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            // معالجة نتيجة MessageBox
            if (result == DialogResult.Yes)
            {
                // إذا وافق المستخدم على الخروج دون حفظ، إغلاق النموذج
                // لا حاجة لفعل أي شيء، النموذج سيغلق تلقائيًا
            }
            else if (result == DialogResult.No)
            {
                // إذا اختار المستخدم No، إلغاء إغلاق النموذج
                e.Cancel = true;
            }
            // إذا اختار No، لا تفعل شيئًا (يستمر المستخدم في التعديل)
        }




    }

}
