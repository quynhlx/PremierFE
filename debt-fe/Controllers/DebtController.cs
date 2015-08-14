using debt_fe.Models;
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
            var Debt = DebtModel.ReadXML("~/XMLData/DebtData.xml", typeof(List<DebtModel>));
            return View(Debt);           
        }
        public ActionResult Detail(int id)
        {
            var Debts = DebtModel.ReadXML("~/XMLData/DebtData.xml", typeof(List<DebtModel>));
            var Debt = ((List<DebtModel>)Debts).Single(p => p.Id == id);
            return PartialView("Detail", Debt);
        }
    }
}