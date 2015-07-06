using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class MyAccountController : Controller
    {        
        public ActionResult Index()
        {
            // ViewBag.Title = "My Account";

            return View();
        }
    }
}