using debt_fe.Businesses;
using debt_fe.DataAccessHelper;
using debt_fe.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
	[Authorize]
    public class DocumentController : Controller
    {
		private DataProvider _dataProvider;
        private DocumentBusiness _docBusiness;

		public DocumentController()
		{
			_dataProvider = new DataProvider("tbone", "tbone");
            _docBusiness = new DocumentBusiness();
		}

        public ActionResult Index(int? memberISN)
        {
            
            //
            // add member isn to session
			if (memberISN == null)
			{
                int? memberISNSession = (int?)Session["debt_member_isn"];

                if (memberISNSession == null)
                {
                    // return HttpNotFound();				
                    return RedirectToAction("Login", "Account");
                }
				else
                {
                    memberISN = memberISNSession.Value;
                }
			}
            else
            {
                Session["debt_member_isn"] = memberISN;
            }

            var test = (int)Session["debt_member_isn"];

			var query = "select * from Vw_Document where MemberISN=@LoginISN";
			var paramNames = new List<string>() { "LoginISN" };
			var paramValues = new ArrayList() { memberISN.Value };

			var table = _dataProvider.ExecuteQuery(query, paramNames, paramValues);

            var documents = _docBusiness.GetList(table);

            return View(documents);
        }

        /// <summary>
        /// load upload document partial view
        /// </summary>
        /// <returns>partial view of document upload</returns>
        public ActionResult Upload()
        {
            return PartialView("_UploadDocument");
        }
    }
}