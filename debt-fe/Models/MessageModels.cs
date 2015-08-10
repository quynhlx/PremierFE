using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace debt_fe.Models
{
    
    public class MessageModels
    {
        public int Id { set; get; }
        public string AddedDate { set; get; }
        public string Subject { set; get; }
        public List<string> LMessages { set; get; }
        public List<string> LMessagesTime { set; get; }
        public List<string> LMessagesBy { set; get; }
        public string Message
        {
            get
            {
                return LMessages.LastOrDefault();
            }
        }
        public DateTime AddedDateTime
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(AddedDate);
                }
                catch
                {
                    return DateTime.Now;
                }
            }
        }
        public string MessageShort
        {
            get
            {
                if (Message.Length > 50)
                    return Message.Substring(0, 50) + "...";
                else if (Message.Length > 0)
                    return Message;
                else
                    return "No Message Found";
            }
        }
        public static object ReadXML(string path, Type type)
        {
            string pathFull = HttpContext.Current.Server.MapPath(path);
            try
            {
                if (!File.Exists(pathFull))
                {
                    return null;
                }
                XmlSerializer reader = new XmlSerializer(type, new XmlRootAttribute("Messages"));
                //System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(type);
                //---
                System.IO.StreamReader stream = new System.IO.StreamReader(pathFull);
                //---
                var result = reader.Deserialize(stream);
                stream.Close();

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
            }
        }
    }
}
