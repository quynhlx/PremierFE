using debt_fe.DataAccessHelper;
using debt_fe.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using debt_fe.Utilities;
using System.Web.Security;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace debt_fe.Controllers
{
    public class AccountController : Controller
    {
        private DataProvider _dataProvider;

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

			var dealerISN = int.Parse(ConfigurationManager.AppSettings["DealerISN"]);
			// var backdoorPwd = "";

			var paramNames = new List<string>
			{
				"username", "password", "dealerisn"
			};

			var paramValues = new ArrayList
			{
				model.Username, Utility.ToMD5Hash(model.Password), dealerISN
			};

			int clientISN;
			var clientInfo = _dataProvider.ExecuteStoreProcedure("xp_debtext_client_login", paramNames, paramValues, out clientISN);

			if (clientISN > 0)
			{
				var claims = new List<Claim>();
				claims.Add(new Claim(ClaimTypes.Name, model.Username));
				claims.Add(new Claim(ClaimTypes.Hash, Utility.ToMD5Hash(model.Password)));

				var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

				var context = Request.GetOwinContext();
				var authenticationManager = context.Authentication;

				authenticationManager.SignIn(id);

				//
				// login success
				return RedirectToAction("Index", "Document", routeValues: new { memberISN = clientISN});
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
    }
}