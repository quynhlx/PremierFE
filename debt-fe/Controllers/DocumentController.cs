using debt_fe.DataAccessHelper;
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

		public DocumentController()
		{
			_dataProvider = new DataProvider("tbone", "tbone");
		}

        public ActionResult Index(int? memberISN)
        {
			if (memberISN == null)
			{
				// return HttpNotFound();
				memberISN = 9;
			}

			var query = "select * from Vw_Document where MemberISN=@LoginISN";
			var paramNames = new List<string>() { "LoginISN" };
			var paramValues = new ArrayList() { memberISN.Value };

			var table = _dataProvider.ExecuteQuery(query, paramNames, paramValues);

			

            return View();
        }
    }
}