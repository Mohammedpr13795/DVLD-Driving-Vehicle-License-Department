using DVLD.Controls;
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
using static DVLD.Controls.ctrlPersonCardWithFilter;

namespace DVLD.Licenses.International_License
{
    public partial class frmShowPersonLicenseHistory : Form
    {
        private int _PersonID = -1;

        public frmShowPersonLicenseHistory()
        {
            InitializeComponent();
           

        }

        public frmShowPersonLicenseHistory(int PersonID)
        {
            InitializeComponent();
            _PersonID = PersonID;   
        }

        private async void frmShowPersonLicenseHistory_Load(object sender, EventArgs e)
        {

            if (_PersonID!= -1)
            {
                await ctrlPersonCardWithFilter1.LoadPersonInfo(_PersonID);
                ctrlPersonCardWithFilter1.FilterEnabled = false;
                await ctrlDriverLicenses1.LoadInfoByPersonID(_PersonID);
            } 
                else
            {
                ctrlPersonCardWithFilter1.Enabled= true;
                ctrlPersonCardWithFilter1.FilterFocus();
            }
            
           
            
        }

        private async void ctrlPersonCardWithFilter1_OnPersonSelected(object seender , ctrlPersonCardWithFilterEventArgs e)
        {
            _PersonID= e.PersonID;
            if (_PersonID==-1)
            {
                ctrlDriverLicenses1.Clear();
            }
            else
                await ctrlDriverLicenses1.LoadInfoByPersonID(_PersonID);

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
