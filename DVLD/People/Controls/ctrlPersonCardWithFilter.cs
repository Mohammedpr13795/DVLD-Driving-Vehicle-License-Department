using DVLD.People;
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

namespace DVLD.Controls
{
    public partial class ctrlPersonCardWithFilter : UserControl
    {
        
        //// Define a custom event handler delegate with parameters
        //public event Action<int> OnPersonSelected;
        //// Create a protected method to raise the event with a parameter
        //protected virtual void PersonSelected(int PersonID)
        //{
        //    Action<int> handler = OnPersonSelected; 
        //    if (handler != null)
        //    {
        //        handler(PersonID); // Raise the event with the parameter
        //    }
        //}


        public class ctrlPersonCardWithFilterEventArgs : EventArgs
        {
            public int PersonID { get; }

            public ctrlPersonCardWithFilterEventArgs(int personID)
            {
                PersonID = personID;
            }
        }


        public event EventHandler<ctrlPersonCardWithFilterEventArgs> OnPersonSelected;


        protected virtual void RaiseOnPersonSelected(int personid)
        {
            RaiseOnPersonSelected(new ctrlPersonCardWithFilterEventArgs(personid));
        }
        protected virtual void RaiseOnPersonSelected(ctrlPersonCardWithFilterEventArgs e)
        {
            OnPersonSelected.Invoke(this, e);
        }

        private bool _ShowAddPerson=true;
       public bool ShowAddPerson
        {
            get
            {
                return _ShowAddPerson;
            }
            set
            {
                _ShowAddPerson=value;
                btnAddNewPerson.Visible = _ShowAddPerson;
            }
        }

        private bool _FilterEnabled = true;
        public bool FilterEnabled
        {
            get
            {
                return _FilterEnabled;
            }
            set
            {
                _FilterEnabled = value;
                gbFilters.Enabled = _FilterEnabled;
            }
        }

        // My Method And Property

        enum enFillterBy : byte
        {
            NationalNO = 0,
            PersonID,
        }

        public ctrlPersonCardWithFilter()
        {
            InitializeComponent();
        }

       
        //private int _PersonID = -1;

        // Read Only
        public int PersonID  
        {
            get { return ctrlPersonCard1.PersonID; }   
        }

        public clsPerson SelectedPersonInfo
        {
            get { return ctrlPersonCard1.SelectedPersonInfo; }
        }

        public async Task LoadPersonInfo(int PersonID)
        {

            cbFilterBy.SelectedIndex=1;
            txtFilterValue.Text = PersonID.ToString();
            await _FindNow();

        }

        private async Task _FindNow()
        {
            switch ((enFillterBy)cbFilterBy.SelectedIndex)
            {
                case enFillterBy.PersonID:
                    await ctrlPersonCard1.LoadPersonInfo(int.Parse(txtFilterValue.Text));
                   
                    break;

                case enFillterBy.NationalNO:
                    await ctrlPersonCard1.LoadPersonInfo(txtFilterValue.Text);
                    break;

                default:
                    break;
            }
           
            if ( OnPersonSelected !=null && FilterEnabled)
                // Raise the event with a parameter
                RaiseOnPersonSelected(ctrlPersonCard1.PersonID);
        }

        private void cbFilterBy_SelectedIndexChanged(object sender, EventArgs e)
        {
                txtFilterValue.Text = "";
                txtFilterValue.Focus();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChildren())
            {
                //Here we dont continue becuase the form is not valid
                MessageBox.Show("Some fileds are not valide!, put the mouse over the red icon(s) to see the error", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }

            _FindNow();
        }

        private void ctrlPersonCardWithFilter_Load(object sender, EventArgs e)
        {
            cbFilterBy.SelectedIndex= 0;
            txtFilterValue.Focus();
         
        }

        private void txtFilterValue_Validating(object sender, CancelEventArgs e)
        {
            
            if (string.IsNullOrEmpty(txtFilterValue.Text.Trim()))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtFilterValue, "This field is required!");
            }
            else
            {
                //e.Cancel = false;
                errorProvider1.SetError(txtFilterValue, null);
            }
        }

        private void _Subsribe(frmAddUpdatePerson frm)
        {
            frm.DataBack += DataBackEvent; // Subscribe to the event
        }
        private void btnAddNewPerson_Click(object sender, EventArgs e)
        {
            frmAddUpdatePerson frm1 = new frmAddUpdatePerson();
            _Subsribe(frm1);
            frm1.ShowDialog();  

        }

        private async void DataBackEvent(object sender, int PersonID)
        {
            // Handle the data received

            cbFilterBy.SelectedIndex = 1;
            txtFilterValue.Text = PersonID.ToString();    
            await ctrlPersonCard1.LoadPersonInfo(PersonID);
        }

        public void FilterFocus()
        {
            txtFilterValue.Focus();
        }

        private void txtFilterValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed key is Enter (character code 13)
            if (e.KeyChar == (char)13)
            {

                btnFind.PerformClick();
            }

            //this will allow only digits if person id is selected
            if (cbFilterBy.Text == "Person ID")
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);



        }
    }
}
