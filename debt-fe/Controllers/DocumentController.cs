using debt_fe.Businesses;
using debt_fe.DataAccessHelper;
using debt_fe.Models;
using debt_fe.Models.ViewModels;
using debt_fe.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
	[Authorize]
    public class DocumentController : Controller
    {
        private DocumentBusiness _docBusiness;

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
                if (doc.Value < 0)
                {
                    ViewBag.ErrorMessage = "Document already exist.";
                }
                else
                {
                    ViewBag.SuccessMessage = "Document has been added successfully.";
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
			var viewModel = new DocumentViewModel();

			return PartialView("_EditDocument", viewModel);
		}

		[HttpPost]
		public ActionResult UploadDocument(DocumentViewModel viewModel)
		{
			var document = new DocumentModel();

			document.MemberISN = this.MemberISN;
			document.DocName = viewModel.DocName;
			document.Desc = viewModel.Notes;
			document.CreditorISN = viewModel.SelectedCreditorID;
			document.CreditorName = viewModel.GetCreditorName(viewModel.SelectedCreditorID, this.MemberISN);


			var fullPath = string.Empty;
			var canSave = true;

			//
			// save file
			if (viewModel.FileName != null && viewModel.FileName.ContentLength > 0)
			{
				document.FileName = Utility.FormatFileName(viewModel.FileName.FileName);
				document.FileSize = viewModel.FileName.ContentLength;

				var pathConfig = ConfigurationManager.AppSettings["UploadFolder"];
				if (string.IsNullOrEmpty(pathConfig))
				{
					pathConfig = "Uploads";
				}

				//
				// remove first escape
				pathConfig = pathConfig.TrimStart('~', '/');

				//
				// check path is exist
				// if not exist, create new folder
				var path = Server.MapPath("~/" + pathConfig);				

				if (!Directory.Exists(path))
				{
					try
					{
						Directory.CreateDirectory(path);						
					}
					catch(Exception ex)
					{
						canSave = false;
						Debug.WriteLine(ex.Message);
					}
				}

				path = path.TrimEnd('/');
                fullPath = Path.Combine(path, document.FileName);
				
			}

			var documentISN = _docBusiness.UploadDocument(document);

            Session["debt_document_isn"] = documentISN;

            if (documentISN > 0 && canSave)
            {

                //
                // save file if create folder success
                try
                {
                    viewModel.FileName.SaveAs(fullPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

			return RedirectToAction("Index");
		}

    }
}