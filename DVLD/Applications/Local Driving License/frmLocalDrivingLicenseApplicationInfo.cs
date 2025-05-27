using DVLD.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.Applications
{
    public partial class frmLocalDrivingLicenseApplicationInfo : Form
    {
        private int _LDLAppID=-1;

        public frmLocalDrivingLicenseApplicationInfo(int LDLAppID)
        {
            InitializeComponent();
            _LDLAppID= LDLAppID;

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private async void frmLocalDrivingLicenseApplicationInfo_Load(object sender, EventArgs e)
        {
            await ctrlDrivingLicenseApplicationInfo1.LoadApplicationInfoByLocalDrivingAppID(_LDLAppID);
        }
    }
}
