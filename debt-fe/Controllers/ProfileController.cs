using debt_fe.DataAccessHelper;
using debt_fe.Models;
using debt_fe.Models.ViewModels;
using debt_fe.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class ProfileController : Controller
    {
        private DataProvider _data;
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
            }
            set
            {
                _memberISN = value;

                // Session["debt_member_isn"] = _memberISN;
                var debt = Request.Cookies["debt_extension"];
                if (debt == null)
                {
                    debt = new HttpCookie("debt_extension");
                    debt.Expires = DateTime.Now.AddDays(7);
                }

                debt.Values["memberId"] = _memberISN.ToString();

                Response.AppendCookie(debt);
            }
        }
        public ProfileController()
        {
            _data = new DataProvider("tbone", "tbone");
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
            if (Session["ManagementAccount"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            //DataSet ds = LoadData(this.MemberISN, out returnValue);
            var myProfile = new MyProfileViewModal();
            myProfile.GetMyProfile(this.MemberISN);
            return View(myProfile);
        }              

        private DataSet LoadData (int memberISN, out int returnValue)
        {
            var store = "xp_debtuser_getinfo";
            var parameters = new Hashtable();
            parameters.Add("MemberISN", memberISN);
            DataSet ds = _data.ExecuteStoreProcedure(store, parameters, out returnValue);
            return ds;
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
        
        [HttpPost] 
        public ActionResult ChangeRequest (FormCollection Form)
        {
            string content = Form.Get("request-content");
            var db = new PremierEntities();
            try
            {
                var rs = db.xp_client_profile_requestchange(this.MemberISN, content);
                TempData["success"] = "Change Request Successfully";
            }
            catch
            {
                TempData["error"] = "Change Request Error";
            }
            return RedirectToAction("MyProfile");
        }
    }
}