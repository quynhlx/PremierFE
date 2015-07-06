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

			// FormsAuthentication.HashPasswordForStoringInConfigFile(string, "MD5");
			//var p1 =FormsAuthentication.HashPasswordForStoringInConfigFile(model.Password, "MD5");
			//var p2=Utility.ToMD5Hash_WithDash(model.Password);
			//var p3 = Utility.ToMD5Hash(model.Password);

			//var p = p1.Equals(p2);

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

			var loginResult = _dataProvider.ExecuteStoreProcedure("xp_debtext_client_login", paramNames, paramValues);

            return View();
        }
    }
}