using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult BankAccount()
        {
            return View();
        }

        public ActionResult ContactInformation()
        {
            return View();
        }
    }
}