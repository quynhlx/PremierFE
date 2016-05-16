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
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using debt_fe.Models.ViewModels;
using System.Net;
using System.Text;
using System.IO;
using NLog;
using Net.Code.ADONet;
using debt_fe.SignInManager;
using System.Threading.Tasks;

namespace debt_fe.Controllers
{
    public class Account1Controller : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private PremierSignInManager _signInManager;
        private PremierUserManager _userManager;

        public Account1Controller()
        {
            this._dataProvider = new DataAccessHelper.DataProvider("tbone", "tbone");
        }

        public Account1Controller(PremierUserManager userManager, PremierSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

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


        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return Redirect(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    if (!await SignInManager.SendTwoFactorCodeAsync("Phone Code"))
                    {
                        ModelState.AddModelError("", "Send Two Factor Code Failed.");
                        return View("model");
                    }
                    return RedirectToAction("VerifyCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // POST: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl)
        {
            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync("Phone Code"))
            {
                return Json(new { msg = "Request too long ,Please login again", code = -2 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "The authentication code had sent to your phone", code = 1 }, JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }
        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return Redirect(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
































        private DataProvider _dataProvider; 

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

                model.Username, Utility.ToMD5Hash(model.Password.Trim()), dealers
            };

            int clientISN;
            var clientInfo = _dataProvider.ExecuteStoreProcedure("xp_debtext_client_login", paramNames, paramValues, out clientISN);
            //---
            bool IsMFARequired = true;
            if (clientISN > 0)
            {
                //Lay thong tin DealerISN cua Client vua dang nhap.
                var dealerISN = 0;
                if (clientInfo.Tables.Count > 0)
                {
                    if (clientInfo.Tables[0].Rows.Count > 0)
                    {
                        try
                        {
                            dealerISN = Convert.ToInt32(clientInfo.Tables[0].Rows[0]["DealerISN"]);
                        }
                        catch { }
                    }


                }
                //
                var query = "xp_debtuser_getinfo";
                var parameters = new Hashtable();
                parameters.Add("MemberISN", MemberISN);

                var dataProvider = new DataProvider();
                var rsInt = 0;
                var dsProfile = dataProvider.ExecuteStoreProcedure(query, parameters, out rsInt);
                Session["ClientProfile"] = dsProfile;
                if (dsProfile.Tables[1].Rows.Count > 0)
                {
                    try
                    {
                        var attrRow = dsProfile.Tables[1].Select("attID = 'ClientRequiredMFALogin'");
                        int intMFA = attrRow.Length == 0 ? 1 : Convert.ToInt32(attrRow[0]["attValue"]);
                        if(intMFA == 0)
                        {
                            IsMFARequired = false;
                        }
                        else
                        {
                            IsMFARequired = true;
                        }
                    }
                    catch { }

                }

                //
                var parameters2 = new Hashtable();
                parameters2.Add("MemberISN", dealerISN);
                var ds = _dataProvider.ExecuteQuery("select * from Vw_Member where MemberISN = @MemberISN", parameters2);
                Session["DealerProfile"] = ds;

                //
                Session["CurrentPassword"] = model.Password.Trim();
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

                //
                //GET DealerISN from ProfileMember
                //

                if (isWhiteIP || !IsMFARequired)
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
                var Message = string.Format("Authentication Code: {0}", smsCode);
                var rs = SendSMS(Profile.CellPhone, Message);
                Session.Add("SMSCode", smsCode);
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
                        errMsg = "Invalid Username or Password.";
                        break;
                    case -3:
                        errMsg = "Account is inactive";
                        break;
                    default:
                        errMsg = "Invalid Username or Password.";
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
            if (!UseAuthenticationSMS)
                return RedirectToAction("Index", "Document", routeValues: new { memberISN = this.MemberISN });
            var errMsg = string.Empty;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (Session["SmsCode"] == null)
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
            //var rs = smsService.SendSMSExt(usernameSMS, passwordSMS, DateTime.Now.ToString("yyyyMMddHHmm"), DateTime.Now.ToString("yyyyMMddHHmm"), string.Empty, "+841688166199", 1, Message, emailSMS, string.Empty, 1, string.Empty);
            string inboundDID = string.Empty;
            if (this.DealerProfile != null && this.DealerProfile.Rows.Count > 0)
            {
                inboundDID = this.DealerProfile.Rows[0]["memWorkPhone"].ToString();
            }
            _logger.Debug("inboundDID: {0}, strPhoneTo: {1},Message:  {2}", inboundDID, NumberPhone, Message);
            var rs = SendSMS_Nexmo(inboundDID, NumberPhone, Message);
            return rs;
        }
        public string SendSMS_Nexmo(string strPhoneFrom, string strPhoneTo, string strMessage)
        {
           
            var sReturn = string.Empty;
            try
            {
                string sURL = ConfigurationManager.AppSettings["PhoneAPI_Url"];
                string content = "api_key=" + ConfigurationManager.AppSettings["PhoneAPI_AppKey"] + "&api_secret=" + ConfigurationManager.AppSettings["PhoneAPI_AppPass"];
                content += "&to=" + "1" + strPhoneTo + "&from=" + "1" + strPhoneFrom;
                content += "&text=" + strMessage;
                content += "&callback=" + ConfigurationManager.AppSettings["PhoneAPI_UrlCallback"];
                _logger.Debug(content);
                var request = (HttpWebRequest)WebRequest.Create(sURL);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] _byteVersion = Encoding.UTF8.GetBytes(content);
                request.ContentLength = _byteVersion.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(_byteVersion, 0, _byteVersion.Length);
                stream.Close();
                var response = (HttpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    sReturn = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                sReturn = ex.Message;
                _logger.Error(ex.Message);
            }
            return sReturn;
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
                return Json(new { msg = "Phone number is invalid", code = -1 }, JsonRequestBehavior.AllowGet);
            }
            if (Session["SmsCode"] == null)
            {
                return Json(new { msg = "Request too long ,Please login again", code = -2 }, JsonRequestBehavior.AllowGet);
            }
            string smsCode = Session["SmsCode"].ToString();
            var message = string.Format("Authentication Code: {0}", smsCode);
            var rs = SendSMS(numberPhone, message);
            return Json(new { msg = "The authentication code had sent to your phone", code = 1 }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {

            return View();
        }
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        protected bool isWhiteIP
        {
            get
            {
                var currentIP = this.Request.UserHostAddress.ToString();
                var ds = this._dataProvider.ExecuteQuery("select * from Vw_WhiteIPAddr where whtPortal = @whtPortal and whtIPAddr = @IPAddr and DealerISN = @DealerISN", new List<string> { "whtPortal", "IPAddr", "DealerISN" }, new ArrayList { 1, currentIP, this.Profile.DealerISN });
                if (ds.Rows.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }


    }

}