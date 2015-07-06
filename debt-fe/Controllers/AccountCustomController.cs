using debt_fe.DataAccessHelper;
using debt_fe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class AccountCustomController : Controller
    {
        private DataProvider _dataProvider;

        public AccountCustomController()
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

            

            return View();
        }
    }
}