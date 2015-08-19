using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mail;
using System.Text;
using System.Collections.Specialized;

public partial class SendEmail : System.Web.UI.Page
{
    protected NameValueCollection Config
    {
        get { return System.Configuration.ConfigurationManager.AppSettings; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Clear();

        if (!IsPostBack)
        {
            try
            {
                string sName = Request["name"];
                string sEmail = Request["email"];
                string sSubject = Request["subject"];
                string sMessage = Request["message"];

                if (sName != "" && sEmail != "")
                {
                    string sBody = "";
                    sBody += "Name: " + sName + "<br />";
                    sBody += "Email: " + sEmail + "<br />";
                    sBody += "Subject: " + sSubject + "<br />";
                    sBody += "Message: " + sMessage.Replace("\n", "<br />");

                    this.SendMail(Config["EmailFrom"], Config["EmailTo"], Config["EmailSubject"], sBody);

                    Response.Write("0");
                }
                else
                    Response.Write("-1");
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }

        Response.End();
    }

    public int SendMail(string mailFrom, string mailTo, string subject, string content)
    {
        try
        {
            if (mailTo != string.Empty)
            {
                MailMessage msg = new MailMessage();
                msg.To = mailTo;
                msg.From = mailFrom;//ConfigurationSettings.AppSettings["mailFrom"];
                msg.Body = content;
                msg.Subject = subject;
                msg.BodyEncoding = Encoding.UTF8;
                msg.BodyFormat = MailFormat.Html;



                try
                {
                    SmtpMail.SmtpServer = Config["SMTPServer"];
                    SmtpMail.Send(msg);
                }
                catch (Exception ex)
                {
                }
            }
        }
        catch (Exception)
        { }
        return 0;
    }
}
