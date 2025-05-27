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

namespace DVLD.Licenses.International_Licenses.Controls
{
    public partial class ctrlDriverInternationalLicenseInfo : UserControl
    {
        private int _InternationalLicenseID;
        private clsInternationalLicense _InternationalLicense;
        public ctrlDriverInternationalLicenseInfo()
        {
            InitializeComponent();
        }

        public int InternationalLicenseID
        {
            get { return _InternationalLicenseID; }
        }

        private void _LoadPersonImage()
        {
            if (_InternationalLicense.DriverInfo.PersonInfo.Gendor == 0)
                pbPersonImage.Image = Resources.Male_512;
            else
                pbPersonImage.Image = Resources.Female_512;

            string ImagePath = _InternationalLicense.DriverInfo.PersonInfo.ImagePath;

            if (ImagePath != "")
                if (File.Exists(ImagePath))
                    pbPersonImage.Load(ImagePath);
                else
                    MessageBox.Show("Could not find this image: = " + ImagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        // يجب أن يكون المستدعي لهذه الدالة قادرًا على await Task
        private async Task _LoadPersonImageAsync() // 1. تغيير إلى async Task
        {
            // الجزء الأول: تعيين الصورة الافتراضية بناءً على الجنس (سريع)
            // افتراض أن _InternationalLicense و DriverInfo و PersonInfo ليست null
            if (_InternationalLicense.DriverInfo.PersonInfo.Gendor == 0) // 0 قد يمثل Male
                pbPersonImage.Image = Resources.Male_512;
            else
                pbPersonImage.Image = Resources.Female_512;

            string imagePath = _InternationalLicense.DriverInfo.PersonInfo.ImagePath;

            // التحقق مما إذا كان مسار الصورة موجودًا وغير فارغ
            if (!string.IsNullOrEmpty(imagePath)) // استخدام IsNullOrEmpty أفضل
            {
                bool fileExists = false;
                try
                {
                    // 2. جعل File.Exists غير متزامن
                    fileExists = await Task.Run(() => File.Exists(imagePath));
                }
                catch (Exception ex)
                {
                    // التعامل مع أي استثناءات قد تحدث أثناء Task.Run (نادر لـ File.Exists ولكن ممكن)
                    MessageBox.Show($"An error occurred while checking for the image: {imagePath}\nDetails: {ex.Message}",
                                     "File Check Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // الخروج إذا فشل التحقق
                }

                if (fileExists)
                {
                    try
                    {
                        // 3. جعل تحميل الصورة غير متزامن يدويًا
                        Image loadedImage = await Task.Run(() => Image.FromFile(imagePath));

                        // بعد تحميل الصورة بنجاح، قم بتعيينها إلى PictureBox
                        // هذا آمن إذا تم استدعاء _LoadPersonImageAsync من UI thread وتم انتظاره
                        pbPersonImage.Image = loadedImage;
                    }
                    catch (OutOfMemoryException omEx) // قد يُطلق Image.FromFile هذا الاستثناء لملفات الصور التالفة أو غير المدعومة
                    {
                        MessageBox.Show($"Could not load the image (file might be corrupted or an unsupported format): {imagePath}\nDetails: {omEx.Message}",
                                         "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // ستبقى الصورة الافتراضية معروضة (التي تم تعيينها بناءً على الجنس)
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
            // إذا كان imagePath فارغًا أو null، فستبقى الصورة الافتراضية التي تم تعيينها في البداية.
        }
        public async Task LoadInfo(int InternationalLicenseID)
        {
            _InternationalLicenseID = InternationalLicenseID;
            _InternationalLicense = await clsInternationalLicense.FindAsync(_InternationalLicenseID);
            if (_InternationalLicense == null)
            {
                MessageBox.Show("Could not find Internationa License ID = " + _InternationalLicenseID.ToString(),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _InternationalLicenseID = -1;
                return;
            }

            lblInternationalLicenseID.Text = _InternationalLicense.InternationalLicenseID.ToString();
            lblApplicationID.Text = _InternationalLicense.ApplicationID.ToString();
            lblIsActive.Text = _InternationalLicense.IsActive ? "Yes" : "No";
            lblLocalLicenseID.Text = _InternationalLicense.IssuedUsingLocalLicenseID.ToString();
            lblFullName.Text = _InternationalLicense.DriverInfo.PersonInfo.FullName;
            lblNationalNo.Text = _InternationalLicense.DriverInfo.PersonInfo.NationalNo;
            lblGendor.Text = _InternationalLicense.DriverInfo.PersonInfo.Gendor == 0 ? "Male" : "Female";
            lblDateOfBirth.Text = clsFormat.DateToShort(_InternationalLicense.DriverInfo.PersonInfo.DateOfBirth);

            lblDriverID.Text = _InternationalLicense.DriverID.ToString();
            lblIssueDate.Text = clsFormat.DateToShort(_InternationalLicense.IssueDate);
            lblExpirationDate.Text = clsFormat.DateToShort(_InternationalLicense.ExpirationDate);
           
            await _LoadPersonImageAsync();



        }

     
    }
}
