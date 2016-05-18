using debt_fe.Models;
using debt_fe.Models.ViewModels;
using debt_fe.SignInManager;
using debt_fe.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace debt_fe.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        private debt_fe.DataAccessHelper.DataProvider _dataProvider = new DataAccessHelper.DataProvider();
        private PremierEntities db = new PremierEntities();
        public new MyProfileViewModal Profile
        {
            get
            {
                if (this.MemberISN < 0) return null;
                var profile = new MyProfileViewModal();
                profile.GetMyProfile(this.MemberISN);
                return profile;
            }
        }
       
       
        public int MemberISN
        {
            get
            {
                string UserId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(UserId)) return 0;
                return Convert.ToInt32(UserId);
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

            int numberUnread = db.Vw_PremierMessage.Where(p => p.MemberISN == MemberISN && p.ClientRead == 0).ToList().Count;

            var debt = Request.Cookies["debt_extension"];
            if (debt == null)
            {
                debt = new HttpCookie("debt_extension");
                debt.Expires = DateTime.Now.AddDays(7);
            }

            debt.Values["msgUnread"] = numberUnread.ToString();

            Response.AppendCookie(debt);

            var baseView = new BaseViewModel();
            baseView.HeaderViewModel = HeaderInfo;
            baseView.ManagerViewModel = new Models.ManagementAccountModel(MemberISN);
            ViewBag.BaseViewModel = baseView;
            ViewBag.BaseUrlChat = System.Configuration.ConfigurationManager.AppSettings["BaseUrlChat"];
            ViewBag.MyProfile = Profile;
        }
        public int MobileLogin (string username, string pass)
        {
            //var dealers = ConfigurationManager.AppSettings["Dealers"];
            //var paramNames = new List<string>
            //{
            //    "username", "password", "dealers"
            //};

            //var paramValues = new ArrayList
            //{

            //    username, hasspass, dealers
            //};
            //int clientISN;
            //var clientInfo = _dataProvider.ExecuteStoreProcedure("xp_debtext_client_login", paramNames, paramValues, out clientISN);
            //var cookie = Request.Cookies["debt_extension"];
            //if (cookie == null)
            //{
            //    cookie = new HttpCookie("debt_extension");
            //}

            //cookie.Expires = DateTime.Now.AddDays(7);
            //cookie.Values["memberId"] = clientISN.ToString();
            //Response.AppendCookie(cookie);
            var user = UserManager.FindByName(username);
            if (user == null || !string.Equals(user.PasswordHash, pass, StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }
            SignInManager.SignInAsync(user, true, false);

            return user.ISN;
        }

        public BaseController() { }
        public BaseController(PremierUserManager userManager)
        {
            this.UserManager = userManager;
        }
        private PremierUserManager _userManager;
        public PremierUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().Get<PremierUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private PremierSignInManager _signInManager;
        public PremierSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<PremierSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

    }


}