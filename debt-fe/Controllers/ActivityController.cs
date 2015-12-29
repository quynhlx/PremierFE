using debt_fe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class ActivityController : BaseController
    {
        PremierEntities db = new PremierEntities();
        // GET: Activity
        public ActionResult Index()
        {
            if (MemberISN < 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var activities = db.Vw_PremierActivity.ToList();
            return View(activities);
        }
    }
}