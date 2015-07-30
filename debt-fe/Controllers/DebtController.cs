using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class DebtController : Controller
    {
        // GET: Debt
        public ActionResult Index()
        {
            return View();
        }
    }
}