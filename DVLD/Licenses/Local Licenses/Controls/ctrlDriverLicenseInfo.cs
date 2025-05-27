using DVLD.Classes;
using DVLD.Properties;
using DVLD_Buisness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DVLD.Controls;

namespace DVLD.DriverLicense
{
    public partial class ctrlDriverLicenseInfo : UserControl
    {
        private int _LicenseID;
        private clsLicense _License;

        public ctrlDriverLicenseInfo()
        {
            InitializeComponent();
           
        }

        public int LicenseID
        {
            get { return _LicenseID; }
        }

        public clsLicense SelectedLicenseInfo
        { get { return _License; } }

        enum enGendor : byte
        {
            Male = 0,
            Female
        }

        private void _LoadPersonImage()
        {
            pbPersonImage.Image = ((enGendor)_License.DriverInfo.PersonInfo.Gendor == enGendor.Male) ? Resources.Male_512 : Resources.Female_512;  

            //if ((enGendor)_License.DriverInfo.PersonInfo.Gendor == enGendor.Male)
            //    pbPersonImage.Image = Resources.Male_512;
            //else
            //    pbPersonImage.Image = Resources.Female_512;

            string ImagePath = _License.DriverInfo.PersonInfo.ImagePath;

            if (ImagePath != "")
                if (File.Exists(ImagePath))
                    pbPersonImage.Load(ImagePath);
                else
                    MessageBox.Show("Could not find this image: = " + ImagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        // يجب أن يكون المستدعي لهذه الدالة قادرًا على await Task
        // يجب أن يكون المستدعي لهذه الدالة قادرًا على await Task
        private async Task _LoadPersonImageAsync()
        {
            // الجزء الأول: تعيين الصورة الافتراضية بناءً على الجنس
            pbPersonImage.Image = ((enGendor)_License.DriverInfo.PersonInfo.Gendor == enGendor.Male) ? Resources.Male_512 : Resources.Female_512;

            string imagePath = _License.DriverInfo.PersonInfo.ImagePath;

            if (!string.IsNullOrEmpty(imagePath))
            {
                bool fileExists = false;
                try
                {
                    fileExists = await Task.Run(() => File.Exists(imagePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while checking for the image: {imagePath}\nDetails: {ex.Message}",
                                     "File Check Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (fileExists)
                {
                    try
                    {
                        // تحميل الصورة يدويًا في thread منفصل
                        Image loadedImage = await Task.Run(() => Image.FromFile(imagePath));

                        // بعد تحميل الصورة بنجاح، قم بتعيينها إلى PictureBox
                        // هذا آمن إذا تم استدعاء _LoadPersonImageAsync من UI thread وتم انتظاره
                        pbPersonImage.Image = loadedImage;
                    }
                    catch (OutOfMemoryException omEx) // قد يُطلق Image.FromFile هذا الاستثناء لملفات الصور التالفة أو غير المدعومة
                    {
                        MessageBox.Show($"Could not load the image (file might be corrupted or an unsupported format): {imagePath}\nDetails: {omEx.Message}",
                                         "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // ستبقى الصورة الافتراضية معروضة
                    }
                    catch (Exception ex) // لأي استثناءات أخرى أثناء تحميل الصورة
                    {
                        MessageBox.Show($"Could not load the image: {imagePath}\nDetails: {ex.Message}",
                                         "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // ستبقى الصورة الافتراضية معروضة
                    }
                }
                else
                {
                    MessageBox.Show("Could not find this image: = " + imagePath,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public async Task LoadInfo(int LicenseID)
        {
            _LicenseID = LicenseID;
            _License = await clsLicense.FindAsync(_LicenseID);
            if (_License == null)
            {
                MessageBox.Show("Could not find License ID = " + _LicenseID.ToString(),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _LicenseID = -1;
                return;
            }

            lblLicenseID.Text = _License.LicenseID.ToString();
            lblIsActive.Text = _License.IsActive ? "Yes" : "No";
            lblIsDetained.Text = await _License.IsDetained ? "Yes" : "No";
            lblClass.Text = _License.LicenseClassInfo.ClassName;
            lblFullName.Text = _License.DriverInfo.PersonInfo.FullName;
            lblNationalNo.Text = _License.DriverInfo.PersonInfo.NationalNo;
            lblGendor.Text = _License.DriverInfo.PersonInfo.Gendor ==0 ? "Male":"Female";
            lblDateOfBirth.Text = clsFormat.DateToShort(_License.DriverInfo.PersonInfo.DateOfBirth);

            lblDriverID.Text= _License.DriverID.ToString();
            lblIssueDate.Text = clsFormat.DateToShort(_License.IssueDate);
            lblExpirationDate.Text = clsFormat.DateToShort(_License.ExpirationDate);
            lblIssueReason.Text = _License.IssueReasonText;
            lblNotes.Text= string.IsNullOrEmpty(_License.Notes) ? "No Notes":_License.Notes;
            await _LoadPersonImageAsync();



        }

     
    }
}
