using debt_fe.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using debt_fe.Models;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using debt_fe.DataAccessHelper;
using System.Collections;

namespace debt_fe.Controllers
{
    public class AppointmentController : BaseController
    {
        PremierEntities db = new PremierEntities();
        // GET: Appointment
        public ActionResult Index()
        {
            if (!Authentication)
            {
                return RedirectToAction("Login", "Account");
            }
            var troubleTicket = db.Vw_DebtExt_Appointment.Where(t => t.tblDate.HasValue && t.MemberISN == MemberISN).OrderByDescending(m => m.tblDate).ToList() ;
            var model = new List<AppointmentViewModel>();
            int i = 1;
            foreach (var t in troubleTicket)
            {
                if (!t.tblSubject.ToLower().Contains("credit pull"))
                {
                    var phone = "";
                    if (!string.IsNullOrEmpty(t.StaffPhone)) phone = string.Format("({0})", Regex.Replace(t.StaffPhone.Trim(new Char[] { ' ', '(', ')', '-' }), @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3"));
                    var hasAttachFile = false;
                    if (string.IsNullOrEmpty(t.tblResult)) t.tblResult = "-1";
                    if (!string.IsNullOrEmpty(t.tblAttachFile)) hasAttachFile = true;
                    model.Add(new AppointmentViewModel()
                    {
                        No = i,
                        Datetime = t.tblDate.Value.ToString("MM/dd/yyyy hh:mm tt"),
                        Status = (AppointmentStatus)t.tblStatus,
                        Type = t.tblSubject,
                        Action = (AppointmentAction)Convert.ToInt32(t.tblResult),
                        HasAttachFile = true ? HasAttachFile(t.TroubleTicketISN) > 0 : false,
                        ISN = t.TroubleTicketISN,
                        DownloadFile = SingleFile(t.TroubleTicketISN),
                        Description = t.tblDesc,
                        With = string.Format("{0} {1}", t.StaffName, phone)
                    });
                    i++;
                }
            }
            return View(model);
        }
        public ActionResult Mobile(string username, string hashpass)
        {
            if (this.MobileLogin(username, hashpass) < 0 || MemberISN < 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var troubleTicket = db.Vw_DebtExt_Appointment.Where(t => t.tblDate.HasValue && t.MemberISN == MemberISN).OrderByDescending(m => m.tblDate).ToList();
            var model = new List<AppointmentViewModel>();
            int i = 1;
            foreach (var t in troubleTicket)
            {
                if (!t.tblSubject.ToLower().Contains("credit pull"))
                {
                    var phone = "";
                    if (!string.IsNullOrEmpty(t.StaffPhone)) phone = string.Format("({0})", Regex.Replace(t.StaffPhone.Trim(new Char[] { ' ', '(', ')', '-' }), @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3"));
                    var hasAttachFile = false;
                    if (string.IsNullOrEmpty(t.tblResult)) t.tblResult = "-1";
                    if (!string.IsNullOrEmpty(t.tblAttachFile)) hasAttachFile = true;
                    model.Add(new AppointmentViewModel()
                    {
                        No = i,
                        Datetime = t.tblDate.Value.ToString("MM/dd/yyyy hh:mm tt"),
                        Status = (AppointmentStatus)t.tblStatus,
                        Type = t.tblSubject,
                        Action = (AppointmentAction)Convert.ToInt32(t.tblResult),
                        HasAttachFile = true ? HasAttachFile(t.TroubleTicketISN) > 0 : false,
                        ISN = t.TroubleTicketISN,
                        DownloadFile = SingleFile(t.TroubleTicketISN),
                        Description = t.tblDesc,
                        With = string.Format("{0} {1}", t.StaffName, phone)
                    });
                    i++;
                }
            }
            return View(model);
        }
        int HasAttachFile (int id)
        {
            try
            {
                var countDocs = db.Vw_DebtExt_Document.Count(d => d.CSScheduleISN == id);
                return countDocs;
            }
            catch
            {
                return 0;
            }
          
        }
        int SingleFile(int id)
        {
            try {
                var Docs = db.Vw_DebtExt_Document.Where(d => d.CSScheduleISN == id);
                if (Docs.Count() == 1)
                {
                    return Docs.FirstOrDefault().DocumentISN;
                }
                else return 0;
            }catch
            {
                return 0;
            }
        }
        public class DealerTime
        {
            public short TimeOption { set; get; }
            public byte DayLightSaving { set; get; }
        }
        public ActionResult DownloadDocumentModal(int id)
        {
            var DealerISN = Profile.DealerISN;
            var troubleTicket = db.Vw_DebtExt_Appointment.SingleOrDefault(m => m.TroubleTicketISN == id);
            ViewBag.DateTime = troubleTicket.tblDate.Value.ToString("MM/dd/yyyy hh:mm tt");
            var docs = db.xp_debtext_document_cscheduleISN(id, DealerISN);
            var modals = new List<DocumentDownloadViewModel>();
            var baseDir = ConfigurationSettings.AppSettings["SaveFilesDir"];
            foreach (var doc in docs)
            {
                DocumentDownloadViewModel modal = new DocumentDownloadViewModel() {
                    Id = doc.DocumentISN,
                    FileName = doc.docFileName,
                    UploadedDate = doc.updatedDateInDealer.Value,
                    UpdatedBy = doc.updatedName,
                };
                modals.Add(modal);
            }
            return PartialView("DocumentModal", modals);
        }
        public ActionResult DownloadRecordModal(int id)
        {
            return PartialView("RecordModal");
        }
        public FileResult Download(int id)
        {
            string fileName = string.Empty;
            string contentType = "application/pdf";
            return File(fileName, contentType, "file.pdf");
        }
        public string GetDocumentsPath(int DocumentISN, object AddedDate)
        {
            DateTime date = DateTime.Now;
            if (AddedDate == null)
                AddedDate = db.Documents.Single(d => d.DocumentISN == DocumentISN).docAddedDate;

            if (AddedDate != null) date = Convert.ToDateTime(AddedDate);
            string strPath = System.IO.Path.Combine(date.ToString("yyyyMM"), "Documents", date.ToString("dd"), DocumentISN.ToString());
            return strPath;
        }
        public ActionResult DownloadDocument(int documentISN)
        {
            if (documentISN == 0) return RedirectToAction("Index"); 
            var baseDir = ConfigurationSettings.AppSettings["SaveFilesDir"];
            var docs = db.Vw_DebtExt_Document.Where(m => m.MemberISN == this.MemberISN);
            var doc = docs.FirstOrDefault(d => d.DocumentISN == documentISN);
            var fullPath = System.IO.Path.Combine(baseDir, GetDocumentsPath(documentISN, null), doc.docFileName);
            
            if (!System.IO.File.Exists(fullPath))
            {
                // return HttpNotFound();
                TempData["error"] = string.Format("File not found");

               
            }
            byte[] fileBytes = null;

            try
            {
                fileBytes = System.IO.File.ReadAllBytes(fullPath);
            }
            catch (Exception ex)
            {

                // return HttpNotFound();
                TempData["error"] = string.Format("Cannot download file", doc.docFileName);

                return RedirectToAction("Index");
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, doc.docFileName);
        }

    }
}