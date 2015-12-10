using debt_fe.DataAccessHelper;
using debt_fe.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebPages.Html;

namespace debt_fe.Models
{
    public class MyProfileViewModal
    {
        public DateTime LastRequest { set; get; }
        public Nullable<int> DealerISN { get; set; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string HomePhone { set; get; }
        public string WorkPhone { set; get; }
        public string CellPhone { set; get; }
        public string FaxNumber { set; get; }
        public string Email { set; get; }
        public int _Married { set; get; }
        public SelectList Married
		{
			get
			{
				return new SelectList(
                        new[]
                        {
                            new {Value = -1, Text = "--Select One--"},
                            new {Value = 0,Text="Single"},
                            new {Value = 1,Text="Married"},
                            new {Value = 2,Text="Other"}
                          
                        }, "Value", "Text", _Married);
			}
            set
            {
                this._Married = (int)value.SelectedValue;
            }
		}

        public string BestTimeToContact { set; get; }

        public string Address { set; get; }
        public string City { set; get; }
        public string _State { set; get; }
        public SelectList State { 
            get 
            {
                var states = Utility.GetStates();

                if (states == null)
                {
                    states = new List<StateModel>();
                }
                return new SelectList(states, "Code", "Name", new StateModel(){Code=this._State});
            }
            set
            {
                this._State = value.SelectedValue.ToString();
            }
        }
        public string Zip { set; get; }

        public string Language { set; get; }

        public string CoFirstName { set; get; }
        public string CoLastName { set; get; }
        public string CoAddress { set; get; }
        public string CoCity { set; get; }
        public string _CoState { set; get; }

        
        public SelectList CoState
        {
            get
            {
                var states = Utility.GetStates();

                if (states == null)
                {
                    states = new List<StateModel>();
                }
                return new SelectList(states, "Code", "Name", new StateModel() {Code = _CoState });
            }
            set
            {
                this._CoState = value.SelectedValue.ToString();
            }
        }
        public string CoZip { set; get; }
        public string CoEmail { set; get; }

        public string StatusGetData { set; get; }
        public void GetMyProfile (int MemberISN)
        {
            var db = new PremierEntities();
            var userInfos = db.xp_debtuser_getinfo(MemberISN);
            var userInfo = userInfos.FirstOrDefault();

            var query = "xp_debtuser_getinfo";
            var parameters = new Hashtable();
            parameters.Add("MemberISN", MemberISN);

            var dataProvider = new DataProvider();
            var rsInt = 0;
            var ds = dataProvider.ExecuteStoreProcedure(query, parameters, out rsInt ) ;
            try
            {
                this.FirstName = userInfo.memFirstName;
                this.LastName = userInfo.memLastName;
                this.HomePhone = userInfo.memHomePhone;
                this.WorkPhone = userInfo.memWorkPhone;
                this.CellPhone = userInfo.memPhone;
                this.FaxNumber = userInfo.memFax;
                this.Email = userInfo.memEmail;
                this.DealerISN = userInfo.DealerISN;
                var attrRow = ds.Tables[1].Select("attID = 'BestTimeOfCall'");

                this.BestTimeToContact = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();

                attrRow = ds.Tables[1].Select("attID = 'MarriedStatus'");
                this._Married = attrRow.Length == 0 ? -1 : Convert.ToInt32(attrRow[0]["attValue"]);

                this.Address = userInfo.memAddress;
                this.City = userInfo.memCity;
                this._State = userInfo.memState;
                this.Zip = userInfo.memZip;

                attrRow = ds.Tables[1].Select("attID = 'Language'");
                this.Language = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();

                attrRow = ds.Tables[1].Select("attID = 'CoFirstName'");
                this.CoFirstName = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();
                attrRow = ds.Tables[1].Select("attID = 'CoLastName'");
                this.CoLastName = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();
                attrRow = ds.Tables[1].Select("attID = 'CoAddress'");
                this.CoAddress = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();
                attrRow = ds.Tables[1].Select("attID = 'CoCity'");
                this.CoCity = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();
                attrRow = ds.Tables[1].Select("attID = 'CoState'");
                this._CoState = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();
                attrRow = ds.Tables[1].Select("attID = 'CoZip'");
                this.CoZip = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();
                attrRow = ds.Tables[1].Select("attID = 'CoEmail'");
                this.CoEmail = attrRow.Length == 0 ? string.Empty : attrRow[0]["attValue"].ToString();
            }catch 
                (Exception ex)
            {
                StatusGetData = ex.ToString();
            }
        }

    }
}
