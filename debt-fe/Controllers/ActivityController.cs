using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class ActivityController : BaseController
    {
        // GET: Activity
        public ActionResult Index()
        {
            if (MemberISN < 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
    }
}