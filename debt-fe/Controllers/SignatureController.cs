using debt_fe.Models;
using NLog;
using RightSignatures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class SignatureController : Controller
    {
        PremierEntities _db = new PremierEntities();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        // GET: Signature
        public ActionResult Index(int isn, string token)
        {
            _logger.Info("---start signature from email co-client---");
            _logger.Debug("isn = {0}; token = {1}", isn, token);
            var documentInfo = _db.Vw_DebtExt_Document.SingleOrDefault(d => d.MemberISN == isn && d.docGuid == token);
            if(documentInfo == null)
            {
                _logger.Error("This document is not exist.");
                return RedirectToAction("Result", new { message = "This document is not exist." });
            }
            MyProfileViewModal profile = new MyProfileViewModal();
            profile.GetMyProfile(documentInfo.MemberISN.Value);
            if (documentInfo.docSignatureDate.HasValue && documentInfo.docSignatureDate.Value.AddDays(30) <= DateTime.Now)
            {
                _logger.Info("This document expired to sign.");
                _logger.Debug("This document expired to sign. docSignatureDate = {0}", documentInfo.docSignatureDate.Value);
                return RedirectToAction("Result", new { message = "This document expired to sign." });
            }
            if (!string.IsNullOrEmpty(documentInfo.docFileName))
            {
                _logger.Info("This document has been signed.");
                _logger.Debug("This document has been signed by {0} {1} and {2} {3}.", profile.FirstName, profile.LastName, profile.CoFirstName, profile.CoLastName);
                return RedirectToAction("Result", new { message = string.Format("This document has been signed by {0} {1} and {2} {3}.", profile.FirstName, profile.LastName, profile.CoFirstName, profile.CoLastName) });
            }
            var urlRedirect = Url.Action("SignatureDownload2", "Document");
            var urlBase = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");
            urlRedirect = urlRedirect.TrimEnd('/') + "/" + token + "/" + documentInfo.DocumentISN;
            var scheme = Request.Url.Scheme;
            var host = Request.Url.Host;
            var redirect = string.Format("{0}://{1}/{2}", scheme, host.TrimEnd('/'), urlRedirect);
            _logger.Debug("redirect = {0}.", redirect);

            //var redirect = string.Format("http://localhost:{0}/{1}", Request.Url.Port, urlRedirect);


            var apiKey = ConfigurationManager.AppSettings["RightSignatureApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.Error("Api key not found");
                return Json(new { code = -4, msg = "api key not found" }, JsonRequestBehavior.AllowGet);
            }
            RightSignature.SetApiKey(apiKey);

            string docKey = RightSignature.GetSignerToken(token, "Co-Client", redirect);
            _logger.Debug("docKey = {0}", docKey);

            var signUrlBase = ConfigurationManager.AppSettings["Url_RightSignature"];
            _logger.Debug("signUrlBase = {0}", signUrlBase);

            var signUrl = string.Format("{0}?height=700&rt={1}", signUrlBase, docKey);
            _logger.Debug("signUrl = {0}", signUrl);

            ViewBag.IFrameUrl = signUrl;
            _logger.Info("---return IFrameUrl---");
            return View();
        }
        public ActionResult Result(string message)
        {
            _logger.Info("----Start ActionResult Result---");
            _logger.Debug("Message = {0}", message);
            ViewBag.Message = message;
            return View();
        }

    }
}