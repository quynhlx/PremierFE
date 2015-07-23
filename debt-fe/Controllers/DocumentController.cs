using debt_fe.Businesses;
using debt_fe.Models;
using debt_fe.Models.ViewModels;
using debt_fe.Utilities;
using log4net;
using System;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web.Mvc;
using System.Web;
using RightSignatures;

namespace debt_fe.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private DocumentBusiness _docBusiness;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int _memberISN;

        public int MemberISN
        {
            get
            {
                //
                // first, try to get from url
                var isn = Session["debt_member_isn"];
                
                //
                // if session empty, try to get from request url
                if (isn == null)
                {
                    var isnFromUrl = Request["memberISN"];

                    if (string.IsNullOrEmpty(isnFromUrl))
                    {
                        return -1; 
                    }

                    isn = isnFromUrl;
                }

                return int.Parse(isn.ToString());
            }
            set
            {
                _memberISN = value;

                Session["debt_member_isn"] = _memberISN;
            }
        }

        public DocumentController()
        {
            _docBusiness = new DocumentBusiness();
        }

        public ActionResult Index(int? memberISN)
        {
            //
            // add member isn to session
            if (memberISN == null)
            {
                memberISN = this.MemberISN;

                if (memberISN < 0)
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                this.MemberISN = memberISN.Value;
            }

            //
            // view message when upload document failed
            var doc = (int?)Session["debt_document_isn"];

            if (doc != null)
            {
                var editMode = (bool?)Session["debt_document_edit"];
                if (editMode != null)
                {
                    if (doc.Value == -1)
                    {
                        ViewBag.ErrorMessage = "Cannot update document.";
                    }
                    else
                    {
                        ViewBag.SuccessMessage = "Document has been updated successfully";
                    }

                    Session.Remove("debt_document_edit");
                }
                else
                {
                    if (doc.Value < 0)
                    {
                        ViewBag.ErrorMessage = "Cannot upload document.";
                    }
                    else
                    {
                        ViewBag.SuccessMessage = "Document has been added successfully.";
                    }
                }

                Session.Remove("debt_document_isn");
            }

            var documents = _docBusiness.GetDocuments(this.MemberISN);

            return View(documents);
        }

        public ActionResult Upload()
        {
            var viewModel = new DocumentViewModel(this.MemberISN);

            return PartialView("_UploadDocument", viewModel);
        }

        public ActionResult Edit(int documentISN)
        {
            var viewModel = new DocumentViewModel(this.MemberISN, documentISN);

            return PartialView("_EditDocument", viewModel);
        }

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
                    // pathConfig = "C:\\";
                    pathConfig = Environment.GetLogicalDrives()[0]; // expect C:\\
                }

                //
                // check path is exist
                // if not exist, create new folder

                // var path = Server.MapPath("~/" + pathConfig);
                var docPath = _docBusiness.GetDocumentPath(viewModel.DocumentISN, document.AddedDate);
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
                        _logger.ErrorFormat("Cannot create directory {0}. Error message: {1}",fullPath, ex.Message);
                        // Debug.WriteLine(ex.Message);
                    }
                }

                fullPath = Path.Combine(fullPath.TrimEnd('/', '\\'), document.FileName);
            }
            #endregion

            var success = _docBusiness.EditDocument(document);

            Session["debt_document_edit"] = true;

            if (success)
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
                        _logger.Info("Cannot save file", ex);
                    }
                }
            }
            else
            {
                Session["debt_document_isn"] = -1;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UploadDocument(DocumentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Session["debt_document_isn"] = -1;

                return RedirectToAction("Index");
            }

            var document = new DocumentModel();

            document.Public = true;
            document.MemberISN = this.MemberISN;
            document.DocName = viewModel.DocName;
            document.Desc = viewModel.Notes;

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
                            _logger.Error("Cannot save image to " + fullPath, ex);
                        }
                    }
                    catch (Exception ex)
                    {   
                        _logger.Error("Cannot create directory", ex);
                    }
                }
            }
            #endregion

            return RedirectToAction("Index");
        }

        public ActionResult DownloadDocument(int documentISN)
        {
            var documents = _docBusiness.GetDocuments(this.MemberISN);
            var doc = documents.FirstOrDefault(d => d.ID == documentISN);

            var fileDownloadName = doc.FileName;


            //
            // get upload folder
            var pathConfig = ConfigurationManager.AppSettings["UploadFolder"];
            if (string.IsNullOrEmpty(pathConfig))
            {
                pathConfig = "C:\\";
            }

            //
            // get document folder and combine to root folder
            var docPath = _docBusiness.GetDocumentPath(documentISN, doc.AddedDate);
            var fullPath = Path.Combine(pathConfig, docPath);

            //
            // get file path
            fullPath = Path.Combine(fullPath.TrimEnd('/', '\\'), fileDownloadName);

            if (!System.IO.File.Exists(fullPath))
            {
                return HttpNotFound();
            }


            _logger.Info("download path = " + fullPath);


            //
            // read file and return to download
            byte[] fileBytes = null;

            try
            {
                fileBytes = System.IO.File.ReadAllBytes(fullPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return HttpNotFound();
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileDownloadName);
        }

        public ActionResult Signature(int documentISN)
        {
            //
            // step 01: get template isn from vw_debtext_document
            // step 02: get signature id
            // step 03: get template
            //
            

            //
            // step 01
            var templateISN = _docBusiness.GetTemplateId(documentISN);
            if (templateISN < 0)
            {
                return Json(new { code=-1,msg="Templale not found" }, JsonRequestBehavior.AllowGet);
            }


            templateISN = 32;

            var signName = string.Format("RightSigntureDoc_{0}_{1}", documentISN, DateTime.Now.ToString("MMddyyyy"));

            //
            // step 02
            var signId = _docBusiness.GetSignatureId(documentISN, this.MemberISN, signName);

            // 
            // step 03
            var template = _docBusiness.GetTemplateByDocumentId(documentISN,this.MemberISN, templateISN, signId);

            if (template == null)
            {
                return Json(new { code = -1, msg = "Template not found" }, JsonRequestBehavior.AllowGet);
            }

            var apiKey = ConfigurationManager.AppSettings["RightSignatureApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return Json(new {code=-4,msg="api key not found" }, JsonRequestBehavior.AllowGet);
            }

            RightSignature.SetApiKey(apiKey);

            var docKey = RightSignature.Embedded(
                    Guid_Template: template.SignGuid,
                    RoleName: template.SignerRole,
                    mergeFields: template.MergeFields,
                    NameFile: signName,
                    url_redirect: Url.Action("Index", "Home", new { memberISN = this.MemberISN}),
                    RightSign_ISN: signId.ToString());

            if (string.IsNullOrEmpty(docKey) || string.IsNullOrWhiteSpace(docKey))
            {
                return Json(new { code = -3, msg = "embedded signature fail" }, JsonRequestBehavior.AllowGet);
            }

            if (docKey.Equals("-2"))
            {
                return Json(new { code = -2, msg = "cannot embedded signature" }, JsonRequestBehavior.AllowGet);
            }
            
            var signUrlBase = ConfigurationManager.AppSettings["Url_RightSignature"];
            var signUrl = string.Format("{0}?height=700&rt={1}",signUrlBase, docKey);

            return Json(new { code=1, msg="success", data=signUrl}, JsonRequestBehavior.AllowGet);
        }
    }
}