using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using debt_fe.Models;
namespace debt_fe.Controllers
{
    public class HomeController : BaseController
    {
        PremierEntities db = new PremierEntities();
        public ActionResult Index()
        {
            if (MemberISN < 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult HowTo()
        {
            if (MemberISN < 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Support()
        {
            if (MemberISN < 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult Complaint(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_Complaint");
        }
        public ActionResult Complaint (FormCollection form)
        {
            string url = form.Get("returnUrl").ToString();
            string content = form.Get("content").ToString();
            var rs = db.xp_complaint_ins(MemberISN, content, -MemberISN);
            if(rs > 0)
            {
                TempData["success"] = "The complaint was successfully sent";
            }
            else
            {
                TempData["error"] = "The complaint was error sent";
            }
            return Redirect(url); 
        }
    }
}