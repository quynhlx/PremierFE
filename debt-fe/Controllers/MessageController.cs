using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using debt_fe.Models;
namespace debt_fe.Controllers
{
    public class MessageController : Controller
    {
        PremierEntities db;
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

        public MessageController ()
        {
            db = new PremierEntities();
        }
        // GET: Message
        public ActionResult Index()
        {
            if (Session["ManagementAccount"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            //var Messages = MessageModels.ReadXML("~/XMLData/Message.xml", typeof(List<MessageModels>));
            var Messages = db.Vw_PremierMessage.Where(p => p.MemberISN == MemberISN);
            int numberUnread = db.Vw_PremierMessage.Where(p => p.MemberISN == MemberISN && p.ClientRead == 0).ToList().Count;

            var debt = Request.Cookies["debt_extension"];
            if (debt == null)
            {
                debt = new HttpCookie("debt_extension");
                debt.Expires = DateTime.Now.AddDays(7);
            }

            debt.Values["msgUnread"] = numberUnread.ToString();

            Response.AppendCookie(debt);

            ViewBag.numberUnread = numberUnread;
            return View(Messages);

        }
        public ActionResult Detail(int MessageId )
        {
            //var Messages = MessageModels.ReadXML("~/XMLData/Message.xml", typeof(List<MessageModels>));
            var Message = db.Vw_PremierMessage.FirstOrDefault(p=>p.MessageISN == MessageId);
            return PartialView("_Detail", Message);
        }
        [HttpPost]
        public ActionResult Reply(FormCollection form)
        {
            var MsgINS = Convert.ToInt32(form.Get("MessageISN"));
            var content = form.Get("message-content").ToString();
            var rs =  db.xp_premiermessage_reply(MsgINS, content, null);
            TempData["success"] = "Message has been sent";
            return RedirectToAction("Index");
        }
        public ActionResult ViewAll(int MessageId)
        {
            
            
            var MessageHistorys = db.xp_premiermessage_viewall(MessageId).ToList();
            var msg = db.PremierMessages.Find(MessageId);
            msg.ClientRead = 1;
            db.SaveChanges();
            ViewBag.Subject = msg.MsgSubject;
            return PartialView("_ViewAll", MessageHistorys);
        }
    }
}