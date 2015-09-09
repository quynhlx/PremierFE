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

namespace debt_fe.Controllers
{
    public class AccountController : Controller
    {
        private DataProvider _dataProvider;
        ManagementAccountModel ManagementAccount = new ManagementAccountModel();
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AccountController()
        {
            _dataProvider = new DataProvider("tbone","tbone");            
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
                try
                {
                    ManagementAccount.GetDataFromDataBase(clientISN);
                }
                catch (Exception ex)
                {
                    _logger.InfoFormat("[AccountController]Get Management Account Exception : ", ex.ToString());
                }

                var db = new PremierEntities();
                int numberUnread = db.Vw_PremierMessage.Where(p => p.MemberISN == clientISN && p.ClientRead == 0).ToList().Count;
                ViewBag.numberUnread = numberUnread;

                var cookie = Request.Cookies["debt_extension"];
				if (cookie==null)
				{
					cookie = new HttpCookie("debt_extension");
				}

				cookie.Expires = DateTime.Now.AddDays(7);
                cookie.Values["msgUnread"] = numberUnread.ToString();		
				cookie.Values["memberId"] = clientISN.ToString();
				Response.AppendCookie(cookie);
                Session.Add("ManagementAccount", ManagementAccount);

                _logger.InfoFormat("[AccountController] Login Sucessfully");
				//
				// login success
				// return RedirectToAction("Index", "Document", routeValues: new { memberISN = clientISN });
               
				return RedirectToAction("Index", "Document");
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
			
			if (cookie != null && cookie.Values.HasKeys())
			{
				// cookie.Values[""].ad

				cookie.Expires = DateTime.Now.AddDays(-1);
				Response.AppendCookie(cookie);
			}

            Session.RemoveAll();
            return RedirectToAction("Login", "Account");
        }
    }
}