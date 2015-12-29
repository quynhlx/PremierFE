using debt_fe.Models;
using debt_fe.Models.ViewModels;
using debt_fe.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        private debt_fe.DataAccessHelper.DataProvider _dataProvider = new DataAccessHelper.DataProvider();
        private int _memberISN;
        private bool _useAuthenticationSMS;
        public string MasterCode {
            get
            {
                var code = string.Empty;
                try
                {
                    code = ConfigurationManager.AppSettings["MasterCode"].ToString();
                }
                catch { }
                return code;
            }
        }
        public MyProfileViewModal Profile 
        {
             get {
                 if (this.MemberISN < 0) return null;
                 var profile = new MyProfileViewModal();
                 profile.GetMyProfile(this.MemberISN);
                 return profile;
             }
        }
        public bool UseAuthenticationSMS
        {
            get
            {
                _useAuthenticationSMS = false;
                try
                {
                    _useAuthenticationSMS = Convert.ToBoolean(ConfigurationManager.AppSettings["useAuthenticationSMS"].ToString());
                }
                catch { }
                return _useAuthenticationSMS;
            }
        }
        public bool Authentication { 
            get {

                if (Session["Authentication"] != null &&  Session["Authentication"].ToString() == Utility.ToMD5Hash(this.MemberISN.ToString()))
                    return true;
                else return false;
            } 
        } 
        public int MemberISN
        {
            get
            {

                var debt = Request.Cookies["debt_extension"];

                if (debt == null || string.IsNullOrEmpty(debt.Values["memberId"]))
                {
                    return -1;
                }

                var memberId = debt.Values["memberId"];
                return int.Parse(memberId);
            }
            set
            {
                _memberISN = value;

                // Session["debt_member_isn"] = _memberISN;
                var debt = Request.Cookies["debt_extension"];
                if (debt == null)
                {
                    debt = new HttpCookie("debt_extension");
                    debt.Expires = DateTime.Now.AddDays(7);
                }

                debt.Values["memberId"] = _memberISN.ToString();

                Response.AppendCookie(debt);
            }
        }
        
        private HeaderInfoViewModel _headerInfo;
        public HeaderInfoViewModel HeaderInfo
        {
            get
            {
                if (_headerInfo != null)
                    return _headerInfo;
                var paramNames = new List<string> { "MemberISN" };
                var paramValues = new System.Collections.ArrayList { this.MemberISN };
                var data = _dataProvider.ExecuteQuery("select top 1 * from Ledger where MemberISN=@MemberISN and PaymentType=1 and PaymentStatus=1", paramNames, paramValues);

                if(data.Rows.Count > 0)
                {
                    _headerInfo = new HeaderInfoViewModel()
                    {
                        DraftAmount = Convert.ToDecimal(data.Rows[0]["ProjectedAmount"]),
                        DraftDate = Convert.ToDateTime(data.Rows[0]["ProjectedDate"])
                    };
                }
                else
                {
                    _headerInfo = new HeaderInfoViewModel() { DraftAmount = 0, DraftDate = DateTime.MinValue};
                }
                return _headerInfo;
            }
            set
            {
                _headerInfo = value;
            }
        }

        protected override void OnAuthentication(System.Web.Mvc.Filters.AuthenticationContext filterContext)
        {
            base.OnAuthentication(filterContext);

        }
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var baseView = new BaseViewModel();
            baseView.HeaderViewModel = HeaderInfo;
            baseView.ManagerViewModel = new Models.ManagementAccountModel(MemberISN);
            ViewBag.BaseViewModel = baseView;
            ViewBag.BaseUrlChat = System.Configuration.ConfigurationManager.AppSettings["BaseUrlChat"];
        }
    }


}