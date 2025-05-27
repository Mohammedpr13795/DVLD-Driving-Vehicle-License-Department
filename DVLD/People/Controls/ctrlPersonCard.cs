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
using System.Xml.Linq;
using System.IO;
using DVLD.People;

namespace DVLD.Controls
{
    public partial class ctrlPersonCard : UserControl
    {
        private clsPerson _Person;

        private int  _PersonID=-1;

        // Read Only
        public int PersonID   
        {
            get { return _PersonID; }   
        }

        // Read Only
        public clsPerson SelectedPersonInfo
        {
            get { return _Person; }
        }

        public ctrlPersonCard()
        {
            InitializeComponent();
        }

        public async Task LoadPersonInfo(int PersonID)
        {
            _Person= await clsPerson.FindAsync(PersonID);
            if (_Person == null)
            {
                ResetPersonInfo();
                MessageBox.Show("No Person with PersonID = " + PersonID.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
           
                await _FillPersonInfo();
        }

        public async Task LoadPersonInfo(string NationalNo)
        {
            _Person = await clsPerson.FindAsync(NationalNo);
            if (_Person == null)
            {
                ResetPersonInfo();
                MessageBox.Show("No Person with National No. = " + NationalNo.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
             await _FillPersonInfo();
        }

        private void _LoadPersonImage()
        {
            pbPersonImage.Image = _Person.Gendor == 0 ? Resources.Male_512: Resources.Female_512;

            string ImagePath = _Person.ImagePath;
            if (!string.IsNullOrEmpty(ImagePath))
                if (File.Exists(ImagePath))
                    pbPersonImage.ImageLocation= ImagePath;
                else
                    MessageBox.Show("Could not find this image: = " + ImagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        // يجب أن يكون المستدعي لهذه الدالة قادرًا على await Task
        private async Task _LoadPersonImageAsync()
        {
            // الجزء الأول: تعيين الصورة الافتراضية بناءً على الجنس (سريع ولا يحتاج async)
            pbPersonImage.Image = _Person.Gendor == 0 ? Resources.Male_512 : Resources.Female_512;

            string imagePath = _Person.ImagePath; // تم تصحيح اسم المتغير ليتوافق مع الاستخدام

            // التحقق مما إذا كان مسار الصورة موجودًا وغير فارغ
            if (!string.IsNullOrEmpty(imagePath))
            {
                bool fileExists = false;
                try
                {
                    // الجزء الذي يحتاج إلى async: التحقق من وجود الملف
                    // Task.Run ينقل عملية File.Exists إلى thread pool لتجنب حجب UI thread
                    fileExists = await Task.Run(() => File.Exists(imagePath));
                }
                catch (Exception ex)
                {
                    // يمكنك التعامل مع أي استثناءات قد تحدث أثناء Task.Run (نادر لـ File.Exists ولكن ممكن لأسباب أذونات مثلاً)
                    // في هذا المثال، سأفترض أننا سنعرض رسالة خطأ إذا فشل التحقق بشكل غير متوقع
                    MessageBox.Show($"An error occurred while checking for the image: {imagePath}\nDetails: {ex.Message}",
                                    "Error Loading Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // الخروج من الدالة إذا حدث خطأ فادح في التحقق
                }

                // استكمال المنطق بناءً على نتيجة fileExists
                if (fileExists)
                {
                    pbPersonImage.ImageLocation = imagePath;
                }
                else
                {
                    MessageBox.Show("Could not find this image: = " + imagePath,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // لا يوجد 'else' هنا، إذا كان imagePath فارغًا أو null، فستبقى الصورة الافتراضية التي تم تعيينها أولاً.
        }

        private async Task _FillPersonInfo()
        {
            llEditPersonInfo.Enabled = true;
            _PersonID = _Person.PersonID;
            lblPersonID.Text=_Person.PersonID.ToString();
            lblNationalNo.Text = _Person.NationalNo;
            lblFullName.Text = _Person.FullName;
            lblGendor.Text = _Person.Gendor == 0 ? "Male" : "Female";
            lblEmail.Text = _Person.Email;
            lblPhone.Text = _Person.Phone;
            lblDateOfBirth.Text = _Person.DateOfBirth.ToShortDateString();
            lblCountry.Text= await clsCountry.FindCountryNameByCountryIDAsync( _Person.NationalityCountryID) ;
            lblAddress.Text= _Person.Address;
            await _LoadPersonImageAsync();

           


        }

        public void ResetPersonInfo()
        {
            _PersonID = -1;
            lblPersonID.Text = "[????]";
            lblNationalNo.Text = "[????]";
            lblFullName.Text = "[????]";
            pbGendor.Image = Resources.Man_32;
            lblGendor.Text = "[????]";
            lblEmail.Text = "[????]";
            lblPhone.Text = "[????]";
            lblDateOfBirth.Text = "[????]";
            lblCountry.Text = "[????]";
            lblAddress.Text = "[????]";
            pbPersonImage.Image = Resources.Male_512;
        
        }

        private async void llEditPersonInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmAddUpdatePerson frm = new frmAddUpdatePerson( _PersonID );
            frm.ShowDialog();
            //refresh
            await LoadPersonInfo(_PersonID);
        }

        
    }
}
