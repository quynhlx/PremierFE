using debt_fe.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using debt_fe.Models;
namespace debt_fe.Controllers
{
    public class AppointmentController : BaseController
    {
        PremierEntities db = new PremierEntities();
        // GET: Appointment
        public ActionResult Index()
        {
            var troubleTicket = db.Vw_TroubleTicket.Where(t => t.tblDate.HasValue && t.MemberISN == MemberISN).ToList();
            var model = new List<AppointmentViewModel>();
            int i = 1;
            foreach (var t in troubleTicket)
            {
                if (string.IsNullOrEmpty(t.tblResult)) t.tblResult = "-1";
                model.Add(new AppointmentViewModel() { 
                    No = i,
                    Datetime = t.tblDate.Value.ToString("MM/dd/yyyy hh:mm tt"),
                    Status =  (AppointmentStatus)t.tblStatus ,
                    Type = t.tblSubject,
                    Action = (AppointmentAction)Convert.ToInt32 (t.tblResult)
                });
                i++;
            }
            return View(model);
        }
    }
}