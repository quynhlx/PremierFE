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
        // GET: Message
        public ActionResult Index()
        {
            var Messages = MessageModels.ReadXML("~/XMLData/Message.xml", typeof(List<MessageModels>));
            return View(Messages);

        }
        public ActionResult Detail(string MessageId )
        {
            var Messages = MessageModels.ReadXML("~/XMLData/Message.xml", typeof(List<MessageModels>));
            var Message = ((List<MessageModels>)Messages).Single(p => p.Id == Convert.ToInt16(MessageId));
            return PartialView("_Detail", Message);
        }
        public ActionResult ViewAll(string MessageId)
        {
            var Messages = MessageModels.ReadXML("~/XMLData/Message.xml", typeof(List<MessageModels>));
            var Message = ((List<MessageModels>)Messages).Single(p => p.Id == Convert.ToInt16(MessageId));
            return PartialView("_ViewAll", Message);
        }
    }
}