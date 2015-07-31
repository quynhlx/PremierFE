using debt_fe.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class ProfileController : Controller
    {
        public ProfileController()
        {

        }

        // GET: Profile
        public ActionResult BankAccount()
        {
            return View();
        }

        public ActionResult ContactInformation()
        {
            return View();
        }

        public ActionResult LoadUpdateHistory()
        {
            /*
             * error code
             * 
             * -1: file not found
             * -2: cannot read xml file
			 * -3: data not found
             * */

            var xmlPath = @Url.Content("~/App_Data/contacts.xml");
            var updateHistoryPath = Server.MapPath(xmlPath);


            if (!System.IO.File.Exists(updateHistoryPath))
            {
                return Json(new { code = -1, msg = "Xml file not found" }, JsonRequestBehavior.AllowGet);
            }

            var xml = string.Empty;

            try
            {
                xml = System.IO.File.ReadAllText(updateHistoryPath);
            }
            catch(Exception)
            {
                return Json(new {code=-2,msg="Cannot read xml data" }, JsonRequestBehavior.AllowGet);
            }

            var ds = Utility.ConvertXMLToDataSet(xml);

			if (ds == null)
			{
				return Json(new { code = -3, msg = "Data not found" }, JsonRequestBehavior.AllowGet);
			}

            return Json(new { code=1}, JsonRequestBehavior.AllowGet);

        }
    }
}