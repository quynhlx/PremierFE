using debt_fe.DataAccessHelper;
using debt_fe.Models;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using debt_fe.Utilities;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System;
using log4net;
using System.Linq;
using debt_fe.Models.ViewModels;

namespace debt_fe.Controllers
{
    public class AccountController : BaseController
    {
        private DataProvider _dataProvider;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AccountController()
        {
            _dataProvider = new DataProvider("tbone", "tbone");
        }
        public ActionResult Index ()
        {
            return RedirectToAction("Login");
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dealers = ConfigurationManager.AppSettings["Dealers"];
            // var backdoorPwd = "";

            var paramNames = new List<string>
			{
				"username", "password", "dealers"
			};

            var paramValues = new ArrayList
			{
				model.Username, Utility.ToMD5Hash(model.Password), dealers
			};

            int clientISN;
            var clientInfo = _dataProvider.ExecuteStoreProcedure("xp_debtext_client_login", paramNames, paramValues, out clientISN);
            //---



            if (clientISN > 0)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, model.Username));
                claims.Add(new Claim(ClaimTypes.Hash, Utility.ToMD5Hash(model.Password)));

                var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                var context = Request.GetOwinContext();
                var authenticationManager = context.Authentication;

                authenticationManager.SignIn(id);
                var db = new PremierEntities();
                int numberUnread = db.Vw_PremierMessage.Where(p => p.MemberISN == clientISN && p.ClientRead == 0).ToList().Count;
                ViewBag.numberUnread = numberUnread;

                var cookie = Request.Cookies["debt_extension"];
                if (cookie == null)
                {
                    cookie = new HttpCookie("debt_extension");
                }

                cookie.Expires = DateTime.Now.AddDays(7);
                cookie.Values["msgUnread"] = numberUnread.ToString();
                cookie.Values["memberId"] = clientISN.ToString();
                Response.AppendCookie(cookie);

                if(isWhiteIP)
                {
                    Session.Add("Authentication", Utility.ToMD5Hash(this.MemberISN.ToString()));
                    return RedirectToAction("Index", "Document", routeValues: new { memberISN = this.MemberISN });
                }
                if (Profile == null || Profile.CellPhone == string.Empty)
                {
                    ModelState.AddModelError("", "Phone number is invalid");
                    return View(model);
                }
                var smsCode = RandomSMSCode();
                var Message = string.Format("Authetication Code: {0}", smsCode);
                var rs = SendSMS(Profile.CellPhone, Message);
                Session.Add("SMSCode", smsCode);
                _logger.InfoFormat("[AccountController] Login Sucessfully");
                //
                // login success
                // return RedirectToAction("Index", "Document", routeValues: new { memberISN = clientISN });

                return RedirectToAction("AuthenticationSMS");
            }
            else
            {
                var errMsg = string.Empty;

                switch (clientISN)
                {
                    case -4:
                        errMsg = "Account does not exist";
                        break;
                    case -3:
                        errMsg = "Account is inactive";
                        break;
                    default:
                        errMsg = "Password is incorrect";
                        break;
                }

                ModelState.AddModelError("", errMsg);

                return View();
            }
        }

        [HttpPost]
        public ActionResult LogOff()
        {
            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignOut();

            //
            // delete cookie
            var cookie = Request.Cookies["debt_extension"];
            Session.RemoveAll();
            if (cookie != null && cookie.Values.HasKeys())
            {
                // cookie.Values[""].ad

                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.AppendCookie(cookie);
            }

            Session.RemoveAll();
            return RedirectToAction("Login", "Account");
        }

        public ActionResult AuthenticationSMS()
        {
            var model = new AuthenticationViewModel();
            model.NumberPhone = Profile.CellPhone;
            return View(model);
        }
        [HttpPost]
        public ActionResult AuthenticationSMS(AuthenticationViewModel model)
        {
            if (!UseAuthenticationSMS )
                return RedirectToAction("Index", "Document", routeValues: new { memberISN = this.MemberISN });
            var errMsg = string.Empty;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if(Session["SmsCode"] == null )
                return RedirectToAction("Index", "Account");

            var smsCode = Session["SmsCode"].ToString();

            if (model.SMSCode != smsCode && model.SMSCode != MasterCode)
            {
                errMsg = "Authentication code is invalid";
                ModelState.AddModelError("", errMsg);
                return View(model);
            }
            Session.Add("Authentication", Utility.ToMD5Hash(this.MemberISN.ToString()));
            return RedirectToAction("Index", "Document", routeValues: new { memberISN = this.MemberISN });

        }
        private string SendSMS(string NumberPhone, string Message)
        {
            var usernameSMS = ConfigurationManager.AppSettings["UsernameSMS"].ToString();
            var passwordSMS = ConfigurationManager.AppSettings["PasswordSMS"].ToString();
            var emailSMS = ConfigurationManager.AppSettings["EmailSMS"].ToString();
            debt_fe.SMSService.WSAgentSoapClient smsService = new SMSService.WSAgentSoapClient("WSAgentSoap12");
            var rs = smsService.SendSMSExt(usernameSMS, passwordSMS, DateTime.Now.ToString("yyyyMMddHHmm"), DateTime.Now.ToString("yyyyMMddHHmm"), string.Empty, NumberPhone, 1, Message, emailSMS, string.Empty, 1, string.Empty);
            return rs;
        }
        private string RandomSMSCode()
        {
            string SMSCode = string.Empty;
            Random random = new Random();
            int code = random.Next(100000, 999999);
            SMSCode = code.ToString();
            return SMSCode;
        }
        [HttpGet]
        public ActionResult SendSMSAgain(string numberPhone)
        {
            if (MemberISN < 0 || string.IsNullOrEmpty(numberPhone))
            {
                 return Json(new { msg = "phone number is invalid", code = -1 }, JsonRequestBehavior.AllowGet);
            }
            if(Session["SmsCode"] == null)
            {
                return Json(new { msg = "Request too long ,Please login again", code = -2 }, JsonRequestBehavior.AllowGet);
            }
            string smsCode = Session["SmsCode"].ToString();
            var message = string.Format("Authetication Code: {0}", smsCode);
            var rs= SendSMS(numberPhone, message);
            return Json(new { msg = "The authetication code had sent to your phone", code = 1 }, JsonRequestBehavior.AllowGet);
        }

        protected bool isWhiteIP
        {
            get
            {
                var currentIP = this.Request.UserHostAddress.ToString();
                var ds = this._dataProvider.ExecuteQuery("select * from Vw_WhiteIPAddr where whtPortal = @whtPortal and whtIPAddr = @IPAddr ", new List<string> { "whtPortal", "IPAddr" } , new ArrayList { 1, currentIP });
                if (ds.Rows.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
    }

}