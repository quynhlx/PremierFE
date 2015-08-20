using debt_fe.Models;
using debt_fe.Models.ViewModels;
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
			var states = Utility.GetStates();

			if (states == null)
			{
				states = new List<StateModel>();
			}

			ViewBag.States = new SelectList(states, "Code", "Name");

            return View();
        }

        public ActionResult ContactInformation()
        {
			//
			// state dropdown
			// temp
			// var viewModel = new ContactInformationViewModel();
			var states = Utility.GetStates();

			if (states == null)
			{
				states = new List<StateModel>();
			}

			ViewBag.States = new SelectList(states, "Code", "Name");

            return View();
        }
        public ActionResult MyProfile ()
        {
            var states = Utility.GetStates();

            if (states == null)
            {
                states = new List<StateModel>();
            }

            ViewBag.States = new SelectList(states, "Code", "Name");
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

			#region read-from-xml
			/*
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
			*/
			#endregion

			var histories = new List<ContactInformationUpdateHistoryModel>();

			#region contacts
			var contacts = new List<ContactInformationModel>
			{
				new ContactInformationModel
				{
					Id=1,
					HomePhone="123-456-789",
					WorkPhone="",
					MobilePhone="000-111-222",
					Email="oclockvn@gmail.com",
					City="Ho Chi Minh",
					Zip="123456",
					State="CA",
					Preferred=1,
					BestTimeToContact="10 AM"
				},
				new ContactInformationModel
				{
					Id=2,
					HomePhone="123-456-789",
					WorkPhone="",					
					State="CA",
					Preferred=1,
					BestTimeToContact="10 AM"
				},
				new ContactInformationModel
				{
					Id=3,
					HomePhone="123-456-789",
					WorkPhone="",
					MobilePhone="000-111-222",
					Email="oclockvn@gmail.com",					
					BestTimeToContact="10 AM"
				}
			};
			#endregion

			contacts.ForEach(c => histories.Add(new ContactInformationUpdateHistoryModel
			{
				UpdatedBy = "Tien Quang",
				UpdatedDate = DateTime.Now,
				ContactInformationId = c.Id,
				ContactInfo=c
			}));

			



			return Json(histories, JsonRequestBehavior.AllowGet);
		}
    }
}