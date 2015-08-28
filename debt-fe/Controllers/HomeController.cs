using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["ManagementAccount"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult HowTo()
        {
            if (Session["ManagementAccount"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Support()
        {
            if (Session["ManagementAccount"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}