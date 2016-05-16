using debt_fe.Models;
using debt_fe.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class DebtController : BaseController
    {
        PremierEntities db = new PremierEntities();
        // GET: Debt
        [Authorize]
        public ActionResult Index()
        {
            //var Debt = DebtModel.ReadXML("~/XMLData/DebtData.xml", typeof(List<DebtModel>));
            var debts = db.Vw_DebtExt_Creditor.Where(d => d.cdtIsPush.Value != byte.MinValue && d.DebtRemoved == 0 &&d.MemberISN == this.MemberISN).ToList();
            var model = new List<DebtViewModel>();
            int i = 1;
            foreach (var debt in debts)
            {
                model.Add(new DebtViewModel()
                {
                   CreditorISN = debt.CreditorISN,
                   Id = i,
                   DebtName = debt.cdtName,
                   DebtAmount = debt.cdtBalance.Value,
                   AccountNumber = debt.cdtAcctNo,
                   Collector = debt.cltName,
                   Creditor = debt.Creditor,
                   Status = debt.DebtStatus
                });
                i++;
            }
            return View(model);        
   
        }
        public ActionResult Mobile(string username, string hashpass)
        {
            if (this.MobileLogin(username, hashpass) < 0 || MemberISN < 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //var Debt = DebtModel.ReadXML("~/XMLData/DebtData.xml", typeof(List<DebtModel>));
            var debts = db.Vw_DebtExt_Creditor.Where(d => d.cdtIsPush.Value != byte.MinValue && d.DebtRemoved == 0 && d.MemberISN == this.MemberISN).ToList();
            var model = new List<DebtViewModel>();
            int i = 1;
            foreach (var debt in debts)
            {
                model.Add(new DebtViewModel()
                {
                    CreditorISN = debt.CreditorISN,
                    Id = i,
                    DebtName = debt.cdtName,
                    DebtAmount = debt.cdtBalance.Value,
                    AccountNumber = debt.cdtAcctNo,
                    Collector = debt.cltName,
                    Creditor = debt.Creditor,
                    Status = debt.DebtStatus
                });
                i++;
            }
            return View(model);

        }

        [Authorize]
        public ActionResult Detail(int id)
        {
            var debt = db.Vw_DebtExt_Creditor.SingleOrDefault(d => d.CreditorISN == id);

            var model = new DebtViewModel() {
                CreditorISN = debt.CreditorISN,
                DebtName = debt.cdtName,
                DebtAmount = debt.cdtBalance.Value,
                AccountNumber = debt.cdtAcctNo,
                Collector = debt.cltName,
                Creditor = debt.Creditor,
                Status = debt.DebtStatus
            };

            return PartialView("Detail", model);
        }
    }
}