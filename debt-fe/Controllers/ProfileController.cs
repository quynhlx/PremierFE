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
             * */

            var xmlPath = @Url.Content("~/App_Data/ContactUpdateHistory.xml");
            var updateHistoryPath = string.Empty;

            try
            {
                updateHistoryPath = Server.MapPath(xmlPath);
            }
            catch(Exception)
            {
                return Json(new {code=-3,msg="Cannot find folder path" }, JsonRequestBehavior.AllowGet);
            }


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

            return Json(new { code=1}, JsonRequestBehavior.AllowGet);

        }
    }
}