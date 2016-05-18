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
            var activities = db.Vw_PremierActivity.Where(m => m.MemberISN == MemberISN).OrderByDescending(a=>a.updatedDate);
            return View(activities);
        }
        public ActionResult Mobile(string username, string pass)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (this.MobileLogin(username, pass) < 0)
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            var activities = db.Vw_PremierActivity.Where(m => m.MemberISN == MemberISN).OrderByDescending(a => a.updatedDate);
            return View(activities);
        }
    }
}