using debt_fe.Businesses;
using debt_fe.Models;
using debt_fe.Models.ViewModels;
using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using RightSignatures;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using debt_fe.DataAccessHelper;
using System.Data;
using System.Collections;
using debt_fe.Service;
using NLog;

namespace debt_fe.Controllers
{
    public class DocumentController : BaseController
    {
        private DocumentBusiness _docBusiness;
        private SignatureBusiness _signBusiness;
        private DataProvider db;
        PremierEntities _db = new PremierEntities();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        IPremierBusiness _premierBusiness;
        public DocumentController()
        {
            _premierBusiness = new PremierBusiness();
            _docBusiness = new DocumentBusiness();
            _signBusiness = new SignatureBusiness();
            db = new DataProvider();
        }
        [Authorize]
        public async Task<ActionResult> Index()
        {
            _logger.Info("---Start Index ActionResult---");
            //
            // view message when upload document failed
            var doc = (int?)Session["debt_document_isn"];

            #region check session
            if (doc != null)
            {
                var editMode = (bool?)Session["debt_document_edit"];
                if (editMode != null)
                {
                    if (doc.Value == -1)
                    {
                        TempData["error"] = "Cannot update document.";
                    }
                    else
                    {
                        TempData["success"] = "Document has been updated.";
                    }

                    Session.Remove("debt_document_edit");
                }
                else
                {
                    if (doc.Value < 0)
                    {
                        TempData["error"] = "Cannot upload document.";
                    }
                    else
                    {
                        TempData["success"] = "Document has been added.";
                    }
                }

                Session.Remove("debt_document_isn");
            }
            #endregion
            ViewBag.CoFullName = string.Format("{0} {1}", Profile.CoFirstName, Profile.CoLastName);
            var documents = _premierBusiness.GetDocuments(this.MemberISN).OrderByDescending(d => d.AddedDate);
            //var documents = _docBusiness.GetDocuments(memberId).OrderByDescending(d => d.AddedDate);
            _logger.Info("---End Index ActionResult---");
            ViewBag.MemberISN = MemberISN;
            return View(documents);
        }

        [Authorize]
        public ActionResult Upload()
        {
            var viewModel = new DocumentViewModel(this.MemberISN);

            return PartialView("_UploadDocument", viewModel);
        }

        [Authorize]
        public ActionResult Edit(int documentISN)
        {
            var viewModel = new DocumentViewModel(this.MemberISN, documentISN);

            return PartialView("_EditDocument", viewModel);
        }
        [Authorize]
        [HttpPost]
        public ActionResult Edit(DocumentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Session["debt_document_isn"] = -1;

                return RedirectToAction("Index");
            }

            var document = new DocumentModel();

            document.Public = true;
            document.ID = viewModel.DocumentISN;
            document.MemberISN = this.MemberISN;
            document.Desc = viewModel.Notes;

            document.DocName = viewModel.DocName;

            if (viewModel.SelectedCreditorID != null)
            {
                document.CreditorISN = viewModel.SelectedCreditorID.Value;
                document.CreditorName = viewModel.GetCreditorName(viewModel.SelectedCreditorID.Value, this.MemberISN);
            }

            document.AddedDate = viewModel.AddedDate;

            //
            // TODO
            // save file
            var fullPath = string.Empty;
            var canSave = false;

            //
            // save file
            #region save file
            if (viewModel.UploadedFile != null && viewModel.UploadedFile.ContentLength > 0)
            {
                document.FileName = viewModel.UploadedFile.FileName;
                document.FileSize = viewModel.UploadedFile.ContentLength;

                var pathConfig = ConfigurationManager.AppSettings["UploadFolder"];
                if (string.IsNullOrEmpty(pathConfig))
                {
                    pathConfig = Environment.GetLogicalDrives()[0]; // expect C:\\
                }

                //
                // check path is exist
                // if not exist, create new folder

                // var path = Server.MapPath("~/" + pathConfig);
                // var docPath = _docBusiness.GetDocumentPath(viewModel.DocumentISN, document.AddedDate);
                var docPath = _premierBusiness.GetDocumentPath(viewModel.DocumentISN, document.AddedDate);
                fullPath = Path.Combine(pathConfig, docPath);

                if (!Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullPath);
                        canSave = true;
                    }
                    catch (Exception ex)
                    {
                        canSave = false;
                        _logger.Error(ex.Message, "Cannot create directory {0}.", fullPath);
                    }
                }

                fullPath = Path.Combine(fullPath.TrimEnd('/', '\\'), document.FileName);
                canSave = true;
            }
            #endregion

            var success = _premierBusiness.EditDocument(document);

            Session["debt_document_edit"] = true;

            if (success > 0)
            {
                Session["debt_document_isn"] = document.ID;
                if (canSave)
                {
                    try
                    {
                        viewModel.UploadedFile.SaveAs(fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Cannot save file");
                    }
                }
            }
            else
            {
                Session["debt_document_isn"] = -1;
            }

            return RedirectToAction("Index");
        }
        [Authorize]
        [HttpPost]
        public ActionResult UploadDocument(DocumentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Session["debt_document_isn"] = -1;

                return RedirectToAction("Index");
            }

            var UserIP = string.Empty;
            UserIP = Request.ServerVariables["REMOTE_ADDR"];
            var document = new DocumentModel();

            document.Public = true;
            document.SignatureStatus = 0;
            document.MemberISN = this.MemberISN;
            document.DocName = viewModel.DocName;
            document.Desc = viewModel.Notes;
            document.SendIP = UserIP;
            document.UpdatedBy = -this.MemberISN;
            if (viewModel.SelectedCreditorID != null)
            {
                document.CreditorISN = viewModel.SelectedCreditorID.Value;
                document.CreditorName = viewModel.GetCreditorName(viewModel.SelectedCreditorID.Value, this.MemberISN);
            }

            var addedDate = DateTime.Now;

            var fullPath = string.Empty;

            //
            // get file uploaded
            if (viewModel.UploadedFile != null && viewModel.UploadedFile.ContentLength > 0)
            {
                document.FileName = viewModel.UploadedFile.FileName;
                document.FileSize = viewModel.UploadedFile.ContentLength;
            }

            var documentISN = _docBusiness.UploadDocument(document);

            //
            // save file if add document success
            #region save document isn to session
            Session["debt_document_isn"] = documentISN;

            if (documentISN > 0 && !string.IsNullOrEmpty(document.FileName))
            {

                var pathConfig = ConfigurationManager.AppSettings["UploadFolder"];
                if (string.IsNullOrEmpty(pathConfig))
                {
                    // pathConfig = "C:\\";                   
                    pathConfig = Environment.GetLogicalDrives()[0]; // expect C:\\
                }

                //
                // check path is exist
                // if not exist, create new folder

                // document template
                // yyyyMM\\Documents\\dd\\123
                var docPath = _docBusiness.GetDocumentPath(documentISN, addedDate);
                fullPath = Path.Combine(pathConfig, docPath);

                if (!Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullPath);

                        fullPath = Path.Combine(fullPath.TrimEnd('/', '\\'), document.FileName);

                        //
                        // save file if create folder success
                        try
                        {
                            viewModel.UploadedFile.SaveAs(fullPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Cannot save image to {0}", fullPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Cannot create directory");
                    }
                }
            }
            #endregion

            return RedirectToAction("Index");
        }
        [Authorize]
        public ActionResult DownloadDocument(int documentISN)
        {
            var documents = _docBusiness.GetDocuments(this.MemberISN);
            var doc = documents.FirstOrDefault(d => d.ID == documentISN);

            var fileDownloadName = doc.FileName;


            //
            // get upload folder
            var pathConfig = ConfigurationManager.AppSettings["UploadFolder"];
            _logger.Debug("get pathConfig = " + pathConfig);
            if (string.IsNullOrEmpty(pathConfig))
            {
                pathConfig = Environment.GetLogicalDrives()[0]; // expect C:\\
            }

            //
            // get document folder and combine to root folder
            var docPath = _docBusiness.GetDocumentPath(documentISN, doc.AddedDate);
            _logger.Debug("get docPath = " + docPath);
            var fullPath = Path.Combine(pathConfig, docPath);

            //
            // get file path
            fullPath = Path.Combine(fullPath.TrimEnd('/', '\\'), fileDownloadName);

            if (!System.IO.File.Exists(fullPath))
            {
                // return HttpNotFound();
                TempData["error"] = string.Format("File not found");

                return RedirectToAction("Index");
            }


            _logger.Debug("download path = " + fullPath);


            //
            // read file and return to download
            byte[] fileBytes = null;

            try
            {
                fileBytes = System.IO.File.ReadAllBytes(fullPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);

                // return HttpNotFound();
                TempData["error"] = string.Format("Cannot download file", fileDownloadName);

                return RedirectToAction("Index");
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileDownloadName);
        }
        [Authorize]
        public ActionResult SignatureSendToCoClient(int documentISN)
        {
            _logger.Info("---Open modal Signature Send To CoClient---");
            _logger.Debug("documentISNStr = {0}", documentISN);
            _logger.Debug("CoEmail = {0}", Profile.CoEmail);

            ViewBag.docId = documentISN;
            ViewBag.email = Profile.CoEmail;
            _logger.Info("---return modal Signature Send To CoClient---");
            return PartialView("_Send2YourCoClient");
        }

        [Authorize]
        [HttpPost]
        public ActionResult SignatureSendToCoClient(FormCollection form)
        {
            _logger.Info("---Post to Signature Send To CoClient---");

            string documentISNStr = form.Get("docId");
            string coclientemail = form.Get("coclientemail");
            int docId = Convert.ToInt32(documentISNStr);

            _logger.Debug("documentISNStr = {0}", documentISNStr);
            _logger.Debug("coclientemail = {0}", coclientemail);

            string dealerEmail = _db.Vw_Member.SingleOrDefault(m => m.MemberISN == Profile.DealerISN).memDropID;
            // Nếu như là sent to co-client thì chạy đoạn này
            string Guid_Doc = string.Empty;
            // lay cái token tu DB 

            var doc = _premierBusiness.GetSigntureDocument(docId);
            Guid_Doc = doc.DocGUID;

            //
            var urlToken = Url.Action("Index", "Signature", new { isn = MemberISN, token = Guid_Doc, docId = doc.ID });
            var scheme = Request.Url.Scheme;
            var host = Request.Url.Host;
            var redirectToken = string.Format("{0}://{1}/{2}", scheme, host.TrimEnd('/'), urlToken.TrimStart('/'));
            //var redirectToken = string.Format("http://localhost:{0}/{1}", Request.Url.Port, urlToken);
            _logger.Debug("redirectToken = {0}", redirectToken);
            try
            {
                IEmailService email = new debt_fe.Service.EmailService();
                //email.ConfigEmailService("quynhlx1412@gmail.com", "Code7198");
                email.ConfigEmailService();

                string serverPath = System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplates/");
                var parameters = new Hashtable();
                parameters.Add("{{UrlRedirect}}", redirectToken);
                parameters.Add("{{CoClientFullName}}", Profile.CoFirstName + " " + Profile.CoLastName);
                parameters.Add("{{ClientFullName}}", Profile.FirstName + " " + Profile.LastName);
                email.SetBody(serverPath, "Content-Send-2-Co-Client_to-Sign-via-Right-Signature.htm", parameters);
                email.SendEmailTo(coclientemail);
                _logger.Info("Email has been sent.");
                TempData["success"] = "Email has been sent.";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                TempData["error"] = "Email has been sent failed.";
            }

            string desc = doc.docHistory;
            string notes = string.Empty;
            notes = notes + string.Format("{0:MM/dd/yyyy, hh:mm tt}: {1} \n", DateTime.Now, coclientemail);
            int docID = Convert.ToInt32(documentISNStr);
            _docBusiness.UpdateHistory(docID, notes);
            _logger.Info("---Return Signature Send To CoClient---");

            return RedirectToAction("Index");
        }
        [Authorize]
        public ActionResult Signature(int documentISN)
        {
            _logger.Info("---start signature---");

            _logger.Debug("Document ISN = " + documentISN);


            var documentInfo = _premierBusiness.GetSigntureDocument(documentISN);
            if (documentInfo.docSignatureDate.HasValue)
            {
                documentInfo = _premierBusiness.GetSubSigntureDocument(documentISN, true);
            }
            if (documentInfo == null)
            {
                return Json(new { code = -5, msg = "Document ISN not found" }, JsonRequestBehavior.AllowGet);
            }
            documentISN = documentInfo.ID;
            //
            // step 01: get template isn from vw_debtext_document
            // step 02: get signature id
            // step 03: get template
            //


            //
            // step 01

            var templateISN = _docBusiness.GetTemplateId(documentInfo.ID);

            //templateISN = 43;

            if (templateISN < 0)
            {
                return Json(new { code = -1, msg = "Templale not found" }, JsonRequestBehavior.AllowGet);
            }

            //var signName = string.Format("Contract{0}_{1}", documentISN, DateTime.Now.ToString("MMddyyyy"));

            var fileTemplate = "NOlimit";

            if (Profile._State.Equals("TX") || Profile._State.Equals("LA") || Profile._State.Equals("WV") || Profile._State.Equals("DE")
                    || Profile._State.Equals("NE") || Profile._State.Equals("CA") || Profile._State.Equals("MO") || Profile._State.Equals("MI") || Profile._State.Equals("OH"))
            {
                fileTemplate = "180daylimit";
            }
            var signName = string.Format("Contract_{0}_{1}_{2}", fileTemplate, Profile.ClientID, DateTime.Now.Ticks.ToString());
            //
            // step 02
            var signId = _docBusiness.GetSignatureId(documentInfo.ID, this.MemberISN, signName);

            // 
            // step 03
            var template = _docBusiness.GetTemplateByDocumentId(documentInfo.ID, this.MemberISN, templateISN, signId);

            if (template == null)
            {
                return Json(new { code = -1, msg = "Template not found" }, JsonRequestBehavior.AllowGet);
            }

            var apiKey = ConfigurationManager.AppSettings["RightSignatureApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                return Json(new { code = -4, msg = "api key not found" }, JsonRequestBehavior.AllowGet);
            }

            RightSignature.SetApiKey(apiKey);

            var urlRedirect = Url.Action("SignatureDownload", "Document", new { docId = documentInfo.ID });
            var scheme = Request.Url.Scheme;
            var host = Request.Url.Host;
            var redirect = string.Format("{0}://{1}/{2}", scheme, host.TrimEnd('/'), urlRedirect.TrimStart('/'));

            // Ở local thì chạy đoạn code này
            //redirect = string.Format("http://localhost:{0}/{1}", Request.Url.Port, urlRedirect);

            var docNoOfSign = documentInfo.docNoOfSign;


            string Guid_Doc = string.Empty;
            string docKey = string.Empty;
            if (!docNoOfSign.HasValue || docNoOfSign.Value == 1)
            {
                docKey = RightSignature.Embedded(
                  Guid_Template: template.SignGuid,
                  RoleName: template.SignerRole,
                  //RoleName: "Client",
                  mergeFields: template.MergeFields,
                  NameFile: signName,
                  url_redirect: redirect,
                  RightSign_ISN: signId.ToString(),
                  Guid_Doc: out Guid_Doc);
            }
            else
            {
                docKey = RightSignature.Embedded(
                    Guid_Template: template.SignGuid,
                    RoleName: "Client",
                    mergeFields: template.MergeFields,
                    NameFile: signName,
                    url_redirect: redirect,
                    RightSign_ISN: signId.ToString(),
                    Guid_Doc: out Guid_Doc);
            }
            _logger.Debug("dockey: {0}", docKey);
            _logger.Debug("Guid_Doc: {0}", Guid_Doc);
            _premierBusiness.UpdateDocGuid(documentInfo.ID, Guid_Doc);

            if (string.IsNullOrEmpty(docKey) || string.IsNullOrWhiteSpace(docKey))
            {
                _logger.Error(string.Format("Cannot get document key. Embedded signature fail."));

                return Json(new { code = -3, msg = "embedded signature fail" }, JsonRequestBehavior.AllowGet);
            }

            if (docKey.Equals("-2"))
            {
                _logger.Error(string.Format("Cannot get document key '{0}'. Embedded signature fail.", docKey));

                return Json(new { code = -2, msg = "cannot embedded signature" }, JsonRequestBehavior.AllowGet);
            }

            var signUrlBase = ConfigurationManager.AppSettings["Url_RightSignature"];
            var signUrl = string.Format("{0}?height=700&rt={1}", signUrlBase, docKey);

            ViewBag.iFrameSrc = signUrl;

            return Json(new { code = 1, msg = "success", data = signUrl, signId = signId, redirect = redirect }, JsonRequestBehavior.AllowGet);

            /*
             * on submit
             * 
             * request data in 'select * from RightSignature_Document where RightSignature_Document_ISN=@signId order by createddate desc'
             * 
             * if sign_status=='signed' && sign_document_guid != ''
             * 
             * then download data
             * 
             * */
        }

        public ActionResult SignatureDownload2(string token, int? docId)
        {
            _logger.Info("---Start Download Signature 2---");
            if (docId.HasValue)
            {
                _logger.Debug("token = {0}; docId = {1}", token, docId.Value);
            }
            else
            {
                _logger.Debug("token = {0}; docId = {1}", token, "null");
                return RedirectToAction("Result", "Signature", new { message = "Document has been error." });

            }
            var documentSignture = _premierBusiness.GetSigntureDocument(docId.Value);
            if (documentSignture == null)
            {
                documentSignture = _premierBusiness.GetSubSigntureDocument(docId.Value, false);
            }
            _logger.Debug("memberISN form Doc = {0}; ", documentSignture.MemberISN);
            var fileTemplate = "NOlimit";
            var myProfile = new MyProfileViewModal();
            myProfile.GetMyProfile(documentSignture.MemberISN);
            if (myProfile._State.Equals("TX") || myProfile._State.Equals("LA") || myProfile._State.Equals("WV") || myProfile._State.Equals("DE")
                    || myProfile._State.Equals("NE") || myProfile._State.Equals("CA") || myProfile._State.Equals("MO") || myProfile._State.Equals("MI") || myProfile._State.Equals("OH"))
            {
                fileTemplate = "180daylimit";
            }
            var strFileName = string.Format("Contract_{0}_{1}_{2}.pdf", fileTemplate, myProfile.ClientID, DateTime.Now.Ticks.ToString());
            var uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];
            var docPath = _docBusiness.GetDocumentPath(docId.Value, null);
            _logger.Debug("strFileName = {0};", strFileName);

            var apiKey = ConfigurationManager.AppSettings["RightSignatureApiKey"];
            _logger.Debug("apiKey = {0}", apiKey);
            if (string.IsNullOrEmpty(apiKey))
            {
                return Json(new { code = -4, msg = "api key not found" }, JsonRequestBehavior.AllowGet);
            }
            RightSignature.SetApiKey(apiKey);
            var urlSigned = RightSignature.GetURLPDFSigned(token);

            _logger.Debug("urlSigned = {0}", urlSigned.UrlFileSigned);
            try
            {
                uploadFolder = Path.Combine(uploadFolder, docPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }

            if (!Directory.Exists(uploadFolder))
            {
                try
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, ex.Message);
                }
            }

            var savePath = Path.Combine(uploadFolder, strFileName);

            string status = string.Empty;
            string statusFile = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                RightSignature.GetStatusDocumentDetail(token, out status, out statusFile);
                _logger.Debug("status = {0}, statusFile = {1}", status, statusFile);
                if (status.Trim() == "signed" && statusFile.Trim() == "done-processing")
                {
                    //
                    // update signature filename after sign
                    var UserIP = string.Empty;
                    var BrowserInfo = string.Format("Browser {0}", Request.Browser.Browser);
                    UserIP = Request.ServerVariables["REMOTE_ADDR"];
                    var currentTime = DateTime.Now;

                    try
                    {
                        _logger.Info("Update Doc Signature");
                        _logger.Debug("UpdateDocSignature({0}, {1}, {2}, {3}, {4})", docId.Value, strFileName, UserIP, BrowserInfo, -documentSignture.MemberISN);
                        _docBusiness.UpdateDocSignature(docId.Value, strFileName, UserIP, BrowserInfo, -documentSignture.MemberISN);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, ex.Message);
                        TempData["error"] = "Document update failed \n" + ex.ToString();
                        return RedirectToAction("Result", "Signature", new { message = "Document has been error." });
                    }
                    _logger.Info("UpdateLeadStatus");
                    _logger.Debug("UpdateLeadStatus ({0}, {1}, {2})", documentSignture.MemberISN, "Contract Received", documentSignture.MemberISN);
                    _docBusiness.UpdateLeadStatus(documentSignture.MemberISN, "Contract Received", documentSignture.MemberISN);

                    break;
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
            DownloadSignatureAsync(urlSigned, savePath);
            var mainDoc = _premierBusiness.GetSigntureDocument(documentSignture.GroupId.Value);

            if (mainDoc.SigntureCompleted)
            {
                AddTemplateDefaut(documentSignture.MemberISN);
                return RedirectToAction("Result", "Signature", new { message = "Document has been signed." });
            }

            return RedirectToAction("Index", "Signature", new { isn = documentSignture.MemberISN, docId = documentSignture.ID });

        }

        [Authorize]
        public ActionResult SignatureDownload(int docId)
        {
            _logger.Info("---Start Download Signature---");
            _logger.Debug("docID = {0}", docId);
            var matchId = string.Empty;

            var documentSignature = _premierBusiness.GetSigntureDocument(docId);
            string Guid_Doc = string.Empty;
            var OneSigntureCompleted = false;
            if (documentSignature == null)
            {

                documentSignature = _premierBusiness.GetSubSigntureDocument(docId, false);
                Guid_Doc = documentSignature.DocGUID;
                if (_premierBusiness.GetStatusSigntureDocument(documentSignature.GroupId.Value) == SigntureStatus.Signed)
                {
                    OneSigntureCompleted = true;
                }
            }
            else
            {
                OneSigntureCompleted = documentSignature.OneSigntureCompleted;
                Guid_Doc = documentSignature.DocGUID;
            }

            var strFileName = GenerateFileName();

            var docPath = _docBusiness.GetDocumentPath(docId, null);
            //
            // update signature filename after sign
            var UserIP = string.Empty;
            var BrowserInfo = string.Format("Browser {0}", Request.Browser.Browser);
            UserIP = Request.ServerVariables["REMOTE_ADDR"];
            var currentTime = DateTime.Now;

            //docNoOfSign
            int? docNoOfSign = documentSignature.docNoOfSign;

            if (!OneSigntureCompleted)
            {
                // ký file đầu tiên docID = groupID
                if (documentSignature.GroupId.HasValue && docId == documentSignature.GroupId.Value)
                {
                    documentSignature.FileName = strFileName;
                    documentSignature.ID = docId;
                    documentSignature.DocGUID = Guid_Doc;
                    if (docNoOfSign.HasValue && docNoOfSign.Value == 1)
                    {
                        int rs = _premierBusiness.DownloadSigntureDocumentFromServer(documentSignature, this.MemberISN);
                        if (rs < 0)
                        {
                            TempData["error"] = "Download signature failed";
                            _premierBusiness.RollBackSigntureDocument(documentSignature.GroupId.Value);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            _docBusiness.UpdateDocSignature(documentSignature.GroupId.Value, strFileName, UserIP, BrowserInfo, -MemberISN, "");
                            TempData["success"] = "Document has been signed.";
                        }
                    }
                    else
                    {
                        _docBusiness.UpdateDocSignature(documentSignature.GroupId.Value, "", UserIP, BrowserInfo, -MemberISN, "");
                        TempData["success"] = "Document has been signed.";
                    }
                }
                // ký file file thứ 2
                else
                {
                    documentSignature.FileName = strFileName;
                    documentSignature.ID = docId;
                    documentSignature.DocGUID = Guid_Doc;
                    if (docNoOfSign.HasValue && docNoOfSign.Value == 1)
                    {
                        int rs1 = _premierBusiness.DownloadSigntureDocumentFromServer(documentSignature, this.MemberISN);
                        if (rs1 < 0)
                        {
                            TempData["error"] = "Download signature failed";
                            //Nên rollback lại document trở về lúc ban đầu. để kí lại
                            _premierBusiness.RollBackSigntureDocument(documentSignature.ID);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                                _docBusiness.UpdateDocSignature(docId, strFileName, UserIP, BrowserInfo, -MemberISN, Guid_Doc);
                                 AddTemplateDefaut(this.MemberISN);
                                TempData["success"] = "Document has been signed.";
                                return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        _docBusiness.UpdateDocSignature(docId, "", UserIP, BrowserInfo, -MemberISN, Guid_Doc);
                        TempData["success"] = "Document has been signed.";
                        return RedirectToAction("Index");
                    }
                }

                var urlRedirect = Url.Action("Signature", "Document", new { documentISN = docId });
                var scheme = Request.Url.Scheme;
                var host = Request.Url.Host;
                var redirect = string.Format("{0}://{1}/{2}", scheme, host.TrimEnd('/'), urlRedirect.TrimStart('/'));
                // Ở local thì chạy đoạn code này
                //redirect = string.Format("http://localhost:{0}/{1}", Request.Url.Port, urlRedirect);

                string funtionSignture = string.Format("signture('{0}',{1});", redirect, docId);

                TempData["FuntionSignture"] = funtionSignture;
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        public void AddTemplateDefaut(int mISN = 0)
        {
            _logger.Info("---Start AddTemplateDefaut---");
            try
            {
                var parameters = new Hashtable();
                parameters.Add("@MemberISN", mISN);
                // var check = new DataProvider().ExecuteQuery("select Count(*) from Vw_DebtExt_Document where MemberISN=@LeadISN and docLastAction='2'", parameters);
                //if (ConvertObjectToInt(check) < 2)
                var check = _db.Vw_DebtExt_Document.Where(m => m.MemberISN == mISN && m.docLastAction == "2").Count();
                if (check >= 2) return;
                var templateId = ConfigurationManager.AppSettings["TeamplateISN_CreateLead"];
                var dsTemplateName = new DataProvider().ExecuteQuery("Select TemplateISN, tplName, tplFile From DebtTemplate Where TemplateISN in (" + templateId + ")");
                if (dsTemplateName.Rows.Count > 0)
                {
                    foreach (DataRow row in dsTemplateName.Rows)
                    {
                        var tempISN = row["TemplateISN"].ToString();
                        var templateName = row["tplName"].ToString();
                        var fileName = row["tplFile"].ToString();
                        var docISN = DocumentEdit(0, mISN, fileName, null, "1", null, null, templateName, 2, -mISN);
                        if (docISN > 0)
                        {
                            var folder = GetTemplatePath(Convert.ToInt32(tempISN), null);
                            folder = Path.Combine(ConfigurationManager.AppSettings["UploadFolder"], folder);
                            var pathFull = Path.Combine(folder, fileName);
                            if (System.IO.File.Exists(pathFull))
                            {
                                var pathSave = Path.Combine(ConfigurationManager.AppSettings["UploadFolder"], this.GetDocumentsPath(docISN, null));
                                if (!Directory.Exists(pathSave)) Directory.CreateDirectory(pathSave);
                                System.IO.File.Copy(pathFull, Path.Combine(pathSave, fileName));
                            }
                        }
                    }
                }
                _logger.Info("---End AddTemplateDefaut---");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }
        private string GetTemplatePath(int TemplateISN, object AddedDate)
        {
            _logger.Info("---Start GetTemplatePath---");
            DateTime date = DateTime.Now;
            if (AddedDate == null)
            {
                var table = db.ExecuteQuery("Select addedDate From DebtTemplate Where TemplateISN=" + TemplateISN.ToString());
                AddedDate = Convert.ToDateTime(table.Rows[0]["addedDate"]);
            }
            if (AddedDate != null) date = Convert.ToDateTime(AddedDate);
            string strPath = date.ToString("yyyyMM") + "\\" + "DebtExtTemplate\\" + date.ToString("dd") + "\\" + TemplateISN.ToString();
            _logger.Info("---End GetTemplatePath---");
            return strPath;
        }
        public string GetDocumentsPath(int DocumentISN, object AddedDate)
        {
            _logger.Info("---Start GetDocumentsPath---");
            DateTime date = DateTime.Now;
            if (AddedDate == null)
            {
                var table = db.ExecuteQuery("Select docAddedDate From Document Where DocumentISN=" + DocumentISN.ToString());
                AddedDate = Convert.ToDateTime(table.Rows[0]["docAddedDate"]);
            }
            if (AddedDate != null) date = Convert.ToDateTime(AddedDate);
            string strPath = date.ToString("yyyyMM") + "\\" + "Documents\\" + date.ToString("dd") + "\\" + DocumentISN.ToString();
            _logger.Info("---End GetDocumentsPath---");
            return strPath;
        }
        private int DocumentEdit(int DocumentISN, int MemberISN, string docFileName, string docSize, string docPublic, string docDesc, string CreditorISN, string docName, int docLast, int updatedBy)
        {
            _logger.Info("---Start DocumentEdit---");
            var result = 0;
            if (string.IsNullOrEmpty(CreditorISN))
            {

                var parameters = new Hashtable();
                parameters.Add("DocumentISN", DocumentISN);
                parameters.Add("MemberISN", MemberISN);
                parameters.Add("docFileName", docFileName);
                parameters.Add("docSize", docSize);
                parameters.Add("docPublic", docPublic);
                parameters.Add("docDesc", docDesc);
                parameters.Add("docName", docName);
                parameters.Add("docLastAction", docLast);
                result = (int)db.ExecuteStoreProcedure("xp_debtext_documenttask_insupd", parameters);

            }
            else
            {
                var parameters = new Hashtable();
                parameters.Add("DocumentISN", DocumentISN);
                parameters.Add("MemberISN", MemberISN);
                parameters.Add("docFileName", docFileName);
                parameters.Add("docSize", docSize);
                parameters.Add("docPublic", docPublic);
                parameters.Add("docDesc", docDesc);
                parameters.Add("CreditorISN", CreditorISN);
                parameters.Add("docName", docName);
                parameters.Add("docLastAction", docLast);
                result = (int)db.ExecuteStoreProcedure("xp_debtext_documenttask_insupd", parameters);
            }
            _logger.Info("---End DocumentEdit---");

            return result;
        }
        [Authorize]
        public ActionResult Message()
        {
            return View();
        }
        private void DownloadSignatureAsync(UrlAfterSigned url, string fullPath)
        {
            _logger.Info("---Start DownloadSignatureAsync---");
            _logger.Debug("url = {0}, fullPath = {1}", url.UrlFileSigned, fullPath);

            Task.Factory.StartNew(new Action(() =>
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        client.DownloadFileTaskAsync(url.UrlFileSigned, fullPath);
                    }
                    catch (Exception ex)
                    {
                        TempData["error"] = "Cannot download file";
                        _logger.Error(ex, ex.Message);

                    }
                }
            }));
            _logger.Info("---End DownloadSignatureAsync---");

        }
        private string GenerateFileName()
        {
            var fileTemplate = "NOlimit";

            if (Profile._State.Equals("TX") || Profile._State.Equals("LA") || Profile._State.Equals("WV") || Profile._State.Equals("DE")
                    || Profile._State.Equals("NE") || Profile._State.Equals("CA") || Profile._State.Equals("MO") || Profile._State.Equals("MI") || Profile._State.Equals("OH"))
            {
                fileTemplate = "180daylimit";
            }
            return string.Format("Contract_{0}_{1}_{2}.pdf", fileTemplate, Profile.ClientID, DateTime.Now.Ticks.ToString());
        }

    }
}