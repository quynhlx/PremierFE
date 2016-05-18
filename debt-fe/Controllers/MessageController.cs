using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using debt_fe.Models;
namespace debt_fe.Controllers
{
    public class MessageController : BaseController
    {
        PremierEntities db;

        public MessageController ()
        {
            db = new PremierEntities();
        }
        // GET: Message
        [Authorize]
        public ActionResult Index()
        {
            //var Messages = MessageModels.ReadXML("~/XMLData/Message.xml", typeof(List<MessageModels>));
            var Messages = db.Vw_PremierMessage.Where(p => p.MemberISN == MemberISN).OrderByDescending(m=>m.addedDate);
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
        public ActionResult Mobile(string username, string pass)
        {
            if(!User.Identity.IsAuthenticated)
            {
                if (this.MobileLogin(username, pass) < 0)
                {
                    return RedirectToAction("Login", "Account");
                }
            }
           
            var Messages = db.Vw_PremierMessage.Where(p => p.MemberISN == MemberISN).OrderByDescending(m => m.addedDate);
            int numberUnread = db.Vw_PremierMessage.Where(p => p.MemberISN == MemberISN && p.ClientRead == 0).ToList().Count;

            var debt = Request.Cookies["debt_extension"];
            if (debt == null)
            {
                debt = new HttpCookie("debt_extension");
                debt.Expires = DateTime.Now.AddDays(7);
            }

            debt.Values["msgUnread"] = numberUnread.ToString();

            Response.AppendCookie(debt);
            TempData["IsMobile"] = true;
            TempData["username"] = username;
            TempData["hashpass"] = pass;
            ViewBag.numberUnread = numberUnread;
            return View(Messages);
        }
        public ActionResult Mobile3 ()
        {
            return View();
        }
        public ActionResult Detail(int MessageId )
        {
            //var Messages = MessageModels.ReadXML("~/XMLData/Message.xml", typeof(List<MessageModels>));
            var Message = db.Vw_PremierMessage.FirstOrDefault(p=>p.MessageISN == MessageId);

            var msg = db.PremierMessages.Find(MessageId);
            if (msg.ClientRead == 0)
            {
                msg.ClientRead = 1;
                db.SaveChanges();
            }

            return PartialView("_Detail", Message);
        }
        public ActionResult NewMessage ()
        {
            return PartialView("_NewMessage");
        }
        [HttpPost]
        public ActionResult NewMessage(FormCollection form)
        {
            var content = form.Get("message-content").ToString();
            var subject = form.Get("subject").ToString();

            var db = Net.Code.ADONet.Db.FromConfig("Premier");
            var sproc = db.StoredProcedure("xp_premiermessage_new").WithParameters(new { MemberISN = this.MemberISN, Subject = subject, Content = content, updatedBy = -this.MemberISN });
            sproc.AsNonQuery();
            TempData["success"] = "Message has been sent";
            if (TempData["IsMobile"] != null && (bool)(TempData["IsMobile"]))
            {
                return RedirectToAction("Mobile", new { username = TempData["username"], hashpass = TempData["hashpass"] });
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Reply(FormCollection form)
        {
            var MsgINS = Convert.ToInt32(form.Get("MessageISN"));
            var content = form.Get("message-content").ToString();
            var rs =  db.xp_premiermessage_reply(MsgINS, content, -this.MemberISN);
            TempData["success"] = "Message has been sent";
            if(TempData["IsMobile"] != null && (bool)(TempData["IsMobile"]))
            {
                return RedirectToAction("Mobile", new { username = TempData["username"] , hashpass= TempData["hashpass"]});
            }
            return RedirectToAction("Index");
        }
        public ActionResult MarkAsRead (int MessageId)
        {
            var msg = db.PremierMessages.Find(MessageId);
            if(msg.ClientRead == 0)
            {
                msg.ClientRead = 1;
                db.SaveChanges();
               
            }
            if (TempData["IsMobile"] != null && (bool)(TempData["IsMobile"]))
            {
                return RedirectToAction("Mobile", new { username = TempData["username"], hashpass = TempData["hashpass"] });
            }
            return RedirectToAction("Index");
        }
        public ActionResult MarkAllRead ()
        {
            var msgs = db.PremierMessages.Where(m => m.ClientRead == 0 && m.MemberISN == this.MemberISN);
            foreach(var msg in msgs)
            {
                    msg.ClientRead = 1;
            }
            db.SaveChanges();
            if (TempData["IsMobile"] != null && (bool)(TempData["IsMobile"]))
            {
                return RedirectToAction("Mobile", new { username = TempData["username"], hashpass = TempData["hashpass"] });
            }
            return RedirectToAction("Index");
        }
        public ActionResult ViewAll(int MessageId)
        {
            var MessageHistorys = db.xp_premiermessage_viewall(MessageId).ToList();
            var msg = db.PremierMessages.Find(MessageId);
            msg.ClientRead = 1;
            db.SaveChanges();
            ViewBag.Subject = msg.MsgSubject;
            return PartialView("_ViewAll", MessageHistorys.OrderByDescending(m=>m.updatedDate));
        }
    }
}