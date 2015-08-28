using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class ActivityController : Controller
    {
        // GET: Activity
        public ActionResult Index()
        {
            if (Session["ManagementAccount"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
    }
}