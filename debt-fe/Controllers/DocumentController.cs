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
				var session = Session["debt_member_isn"];

				if (session == null)
					return -1;

				if (session != null && string.IsNullOrEmpty(session.ToString()))
					return -2;

				return int.Parse(session.ToString());
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

        /// <summary>
        /// load upload document partial view
        /// </summary>
        /// <returns>partial view of document upload</returns>
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

            document.ID = viewModel.DocumentISN;
            document.MemberISN = this.MemberISN;
            document.Desc = viewModel.Notes;
            document.CreditorISN = viewModel.SelectedCreditorID;
            document.DocName = viewModel.DocName;
            document.CreditorName = viewModel.GetCreditorName(viewModel.SelectedCreditorID, this.MemberISN);
            document.AddedDate = viewModel.AddedDate;

            //
            // TODO
            // save file
            var fullPath = string.Empty;
            var canSave = true;

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
                    pathConfig = "C:\\";
                }

                //
                // remove first escape
                // pathConfig = pathConfig.TrimStart('~', '/');

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
                    }
                    catch (Exception ex)
                    {
                        canSave = false;
                        Debug.WriteLine(ex.Message);
                    }
                }
                
                fullPath = Path.Combine(fullPath.TrimEnd('/','\\'), document.FileName);
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
                        // Debug.WriteLine(ex.Message);
                        _logger.Info("Cannot save file",ex);
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

			document.MemberISN = this.MemberISN;
			document.DocName = viewModel.DocName;
			document.Desc = viewModel.Notes;
			document.CreditorISN = viewModel.SelectedCreditorID;
			document.CreditorName = viewModel.GetCreditorName(viewModel.SelectedCreditorID, this.MemberISN);

            var addedDate = DateTime.Now;

			var fullPath = string.Empty;

			//
            // save file
            #region save file
            if (viewModel.UploadedFile != null && viewModel.UploadedFile.ContentLength > 0)
            {
                document.FileName = viewModel.UploadedFile.FileName;
                document.FileSize = viewModel.UploadedFile.ContentLength;
            }
            #endregion


            var documentISN = _docBusiness.UploadDocument(document);

            #region save document isn to session
            Session["debt_document_isn"] = documentISN;

            if (documentISN > 0)
            {

                var pathConfig = ConfigurationManager.AppSettings["UploadFolder"];
                if (string.IsNullOrEmpty(pathConfig))
                {
                    pathConfig = "C:\\";
                }

                //
                // check path is exist
                // if not exist, create new folder
                var docPath = _docBusiness.GetDocumentPath(documentISN, addedDate);
                fullPath = Path.Combine(pathConfig, docPath);

                if (!Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    catch (Exception ex)
                    {
                        // canSave = false;
                        Debug.WriteLine(ex.Message);
                        _logger.Error("Cannot create directory",ex);
                    }
                }

                fullPath = Path.Combine(fullPath.TrimEnd('/', '\\'), document.FileName);


                //
                // save file if create folder success
                try
                {
                    viewModel.UploadedFile.SaveAs(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.Error("Cannot save image "+fullPath,ex);
                    Debug.WriteLine(ex.Message);
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
            

            _logger.Info("download path = "+fullPath);


            //
            // read file and return to download
            byte[] fileBytes = null;

            try
            {
                fileBytes = System.IO.File.ReadAllBytes(fullPath);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return HttpNotFound();
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileDownloadName);
        }
    }
}