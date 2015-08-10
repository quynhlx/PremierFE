using debt_fe.Businesses;
using debt_fe.Models;
using debt_fe.Models.ViewModels;
using log4net;
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

namespace debt_fe.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private DocumentBusiness _docBusiness;
        private SignatureBusiness _signBusiness;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int _memberISN;

        public int MemberISN
        {
            get
            {
				var debt = Request.Cookies["debt_extension"];
				
				if (debt == null || string.IsNullOrEmpty(debt.Values["memberId"]))
				{
					return -1;
				}

				var memberId = debt.Values["memberId"];

				/*
				if (string.IsNullOrEmpty(memberId))
				{
					return -2;
				}
				 */ 

				return int.Parse(memberId);

				#region session
				/*
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
				 */
				#endregion
			}
            set
            {
                _memberISN = value;

                // Session["debt_member_isn"] = _memberISN;
				var debt = Request.Cookies["debt_extension"];
				if (debt==null)
				{
					debt = new HttpCookie("debt_extension");
					debt.Expires = DateTime.Now.AddDays(7);					
				}

				debt.Values["memberId"] = _memberISN.ToString();

				Response.AppendCookie(debt);
            }
        }

        public DocumentController()
        {
            _docBusiness = new DocumentBusiness();
            _signBusiness = new SignatureBusiness();
        }

        public ActionResult Index()
        {
            _logger.InfoFormat("Url: {0}\r\n, abs = {1}\r\n, uri = {2}\r\n, aut = {3}\r\n, fragment = {4}\r\n, host = {5}\r\n, local = {6}\r\n, original = {7}\r\n, port = {8}\r\n, query = {9}\r\n, scheme = {10}\r\n, info = {11}\r\n",
                Request.Url.ToString(),
                Request.Url.AbsolutePath,
                Request.Url.AbsoluteUri,
                Request.Url.Authority,
                Request.Url.Fragment,
                Request.Url.Host,
                Request.Url.LocalPath,
                Request.Url.OriginalString,
                Request.Url.Port,
                Request.Url.Query,
                Request.Url.Scheme,
                Request.Url.UserInfo);

            if (Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.ToString()))
            {
                _logger.InfoFormat("Referred Url: {0}\r\n, abs = {1}\r\n, uri = {2}\r\n, aut = {3}\r\n, fragment = {4}\r\n, host = {5}\r\n, local = {6}\r\n, original = {7}\r\n, port = {8}\r\n, query = {9}\r\n, scheme = {10}\r\n, info = {11}\r\n",
                    Request.UrlReferrer.ToString(),
                    Request.UrlReferrer.AbsolutePath,
                    Request.UrlReferrer.AbsoluteUri,
                    Request.UrlReferrer.Authority,
                    Request.UrlReferrer.Fragment,
                    Request.UrlReferrer.Host,
                    Request.UrlReferrer.LocalPath,
                    Request.UrlReferrer.OriginalString,
                    Request.UrlReferrer.Port,
                    Request.UrlReferrer.Query,
                    Request.UrlReferrer.Scheme,
                    Request.UrlReferrer.UserInfo);

                var urlRedirect = Url.Action("SignatureDownload", "Document", new { signId = 1 });
                var urlPreferred = Request.UrlReferrer.AbsoluteUri;


                _logger.InfoFormat("url redirect = {0}", urlRedirect);
                _logger.InfoFormat("url referred = {0}", urlPreferred);
            }

            


           
            // TempData["info"] = "Hello world";

            //
            // add member isn to session
            #region get member isn

			// var memberISN = this.MemberISN;

			/*
            if (memberISN == null)
            {
				return RedirectToAction("Login", "Account");

                
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
			*/
            #endregion

			var memberId = this.MemberISN;

			if (memberId<0)
			{
				return RedirectToAction("Login", "Account");
			}

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
            #endregion

            var documents = _docBusiness.GetDocuments(memberId);

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
            document.IsSignatureDocument = false;
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
                pathConfig = Environment.GetLogicalDrives()[0]; // expect C:\\
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
                // return HttpNotFound();
                TempData["error"] = string.Format("File not found");

                return RedirectToAction("Index");
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

                // return HttpNotFound();
                TempData["error"] = string.Format("Cannot download file",fileDownloadName);

                return RedirectToAction("Index");
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileDownloadName);
        }

        public ActionResult Signature(string documentISNStr)
        {
            _logger.Info("---start signature---");

            _logger.Info("Document ISN = "+documentISNStr);

            if (string.IsNullOrEmpty(documentISNStr) || string.IsNullOrWhiteSpace(documentISNStr))
            {
                return Json(new { code=-5,msg="Document ISN not found"}, JsonRequestBehavior.AllowGet);
            }

            var documentISN = int.Parse(documentISNStr);

            //
            // step 01: get template isn from vw_debtext_document
            // step 02: get signature id
            // step 03: get template
            //
            

            //
            // step 01

            var templateISN = _docBusiness.GetTemplateId(documentISN);

            // templateISN = 32;

            if (templateISN < 0)
            {
                return Json(new { code=-1,msg="Templale not found" }, JsonRequestBehavior.AllowGet);
            }

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


            _logger.InfoFormat("Url: {0}\r\n, abs = {1}\r\n, uri = {2}\r\n, aut = {3}\r\n, fragment = {4}\r\n, host = {5}\r\n, local = {6}\r\n, original = {7}\r\n, port = {8}\r\n, query = {9}\r\n, scheme = {10}\r\n, info = {11}\r\n",
                Request.Url.ToString(),
                Request.Url.AbsolutePath,
                Request.Url.AbsoluteUri,
                Request.Url.Authority,
                Request.Url.Fragment,
                Request.Url.Host,
                Request.Url.LocalPath,
                Request.Url.OriginalString,
                Request.Url.Port,
                Request.Url.Query,
                Request.Url.Scheme,
                Request.Url.UserInfo);

            if (Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.ToString()))
            {
                _logger.InfoFormat("Referred Url: {0}\r\n, abs = {1}\r\n, uri = {2}\r\n, aut = {3}\r\n, fragment = {4}\r\n, host = {5}\r\n, local = {6}\r\n, original = {7}\r\n, port = {8}\r\n, query = {9}\r\n, scheme = {10}\r\n, info = {11}\r\n",
                    Request.UrlReferrer.ToString(),
                    Request.UrlReferrer.AbsolutePath,
                    Request.UrlReferrer.AbsoluteUri,
                    Request.UrlReferrer.Authority,
                    Request.UrlReferrer.Fragment,
                    Request.UrlReferrer.Host,
                    Request.UrlReferrer.LocalPath,
                    Request.UrlReferrer.OriginalString,
                    Request.UrlReferrer.Port,
                    Request.UrlReferrer.Query,
                    Request.UrlReferrer.Scheme,
                    Request.UrlReferrer.UserInfo);
            }

           var urlRedirect = Url.Action("SignatureDownload", "Document", new { signId =signId});           
           var scheme = Request.Url.Scheme;
            var host = Request.Url.Host;           
            var redirect = string.Format("{0}://{1}/{2}",scheme,host.TrimEnd('/'), urlRedirect.TrimStart('/'));

            var docKey = RightSignature.Embedded(
                    Guid_Template: template.SignGuid,
                    RoleName: template.SignerRole,
                    RecipientName:"Premier",                   
                    mergeFields: template.MergeFields,
                    NameFile: signName,                    
                    url_redirect: redirect,
                    RightSign_ISN: signId.ToString());

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
            var signUrl = string.Format("{0}?height=700&rt={1}",signUrlBase, docKey);

            ViewBag.iFrameSrc = signUrl;

            return Json(new { code = 1, msg = "success", data = signUrl, signId=signId, redirect=redirect }, JsonRequestBehavior.AllowGet);

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

        public ActionResult SignatureDownload(int signId)
        {            
            var matchId = string.Empty;
            var docId = 0;
            var fileName = string.Empty;

            for (int i = 0; i < 10; i++)
            {
                var signTemp = _signBusiness.GetSignatures(this.MemberISN);

                SignatureModel model = null;

                try
                {
                    model = signTemp.Where(s => s.Id == signId).First(s => s.IsSigned && !string.IsNullOrEmpty(s.DocumentGuid));

                    matchId = model.DocumentGuid;
                    fileName = model.DocumentName;
                    
                    if (model.DocumentId!=null)
                    {
                        docId = model.DocumentId.Value;
                    }

                    break;
                }
                catch(Exception)
                {
                    //
                    // not sign yet, give it a try
                    Thread.Sleep(3000);
                }                
            }

            if (string.IsNullOrEmpty(matchId))
            {
                TempData["info"] = "Document Id not found";

                return RedirectToAction("Index");
            }

            var strFileName = string.Format("RightSignatureDoc_{0}.pdf", DateTime.Now.ToString("MMddyyyyhhmmss"));
            var uploadFolder = ConfigurationManager.AppSettings["UploadFolder"];
            var docPath = _docBusiness.GetDocumentPath(docId, null);

            //
            // update signature filename after sign
            _docBusiness.EditSignatureDocument(strFileName, docId);

            var urlSigned = RightSignature.GetURLPDFSigned(matchId);

            try
            {
                uploadFolder = Path.Combine(uploadFolder, docPath);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message,ex);
            }
            
            if (!Directory.Exists(uploadFolder))
            {
                try
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }

            var savePath = Path.Combine(uploadFolder, strFileName);

            /*
            var thread = new Thread(new ParameterizedThreadStart(DownloadSignatureAsync));
            thread.IsBackground = true;
            thread.Start(new ArrayList { urlSigned, savePath });
             * */

            DownloadSignatureAsync(urlSigned, savePath);

            #region
            /*
            using (var client = new WebClient())
            {                 
                try
                {
                    client.DownloadFile(urlSigned.UrlFileSigned, savePath);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }
             */ 

            //
            // read file and return to download
            /*
            byte[] fileBytes = null;

            try
            {
                fileBytes = System.IO.File.ReadAllBytes(savePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            */
            // return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, strFileName);
            #endregion

            return RedirectToAction("Index");
        }

        public ActionResult Message ()
        {
            return View();
        }
        /*
        private async void DownloadSignatureAsync(object parameters)
        {
            var arr = parameters as ArrayList;

            var url = arr[0] as UrlAfterSigned;
            var savePath = (string)arr[1];

            var client = new WebClient();

            await client.DownloadFileTaskAsync(url.UrlFileSigned, savePath);
        }
         */ 

        private void DownloadSignatureAsync(UrlAfterSigned url, string fullPath)
        {
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
                        _logger.Error(ex.Message, ex);
                    }
                }
            }));
        }
        
        
    }
}