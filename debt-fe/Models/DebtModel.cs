using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace debt_fe.Models
{
    public class DebtModel
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string Creditor { set; get; }
        public string AccountNumber { set; get; }
        public string DebtAmount { set; get; }
        public string CurrentCollector { set; get; }
        public string CurrentStatus { set; get; }
        public string Delinquency { set; get; }
        public string Graduation { set; get; }
        public string History { set; get; }
        public string By { set; get; }
        public static object ReadXML(string path, Type type)
        {
            string pathFull = HttpContext.Current.Server.MapPath(path);
            try
            {
                if (!File.Exists(pathFull))
                {
                    return null;
                }
                XmlSerializer reader = new XmlSerializer(type, new XmlRootAttribute("Debt"));
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
