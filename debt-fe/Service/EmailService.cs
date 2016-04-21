using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace debt_fe.Service
{
    public class EmailService : IEmailService
    {
        MailMessage Mail;
        SmtpClient SmtpServer;

        public void ConfigEmailService(string mailFrom,  string password, string mailSubject = "Test Mail", string mailServer = "smtp.gmail.com")
        {
            Mail = new MailMessage();
            SmtpServer = new SmtpClient(mailServer);
            Mail.From = new MailAddress(mailFrom);
            Mail.Subject = mailSubject;
            Mail.IsBodyHtml = true;
            Mail.Body = "This is for testing SMTP mail from GMAIL";
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential(mailFrom, password);
            SmtpServer.EnableSsl = true;
        }
        public void SendEmailTo(params string[] emails)
        {
            try
            {
                foreach (var email in emails)
                {
                    Mail.To.Add(email);
                }
                SmtpServer.Send(Mail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetBody(string contentBody)
        {
            Mail.Body = contentBody;
        }

        public void SetBody(string serverPath,string templateName, Hashtable parameters, bool fromTemplate = true)
        {
            var filePath = System.IO.Path.Combine(serverPath, templateName);
            string text = System.IO.File.ReadAllText(filePath);
            foreach (DictionaryEntry param in parameters)
            {
                var name = param.Key.ToString().TrimStart('@');
                var value = param.Value.ToString();
                text = text.Replace(name, value);
            }

            Mail.Body = text;
        }

        void IEmailService.ConfigEmailService()
        {
            Mail = new MailMessage();
            string mailFrom = ConfigurationManager.AppSettings["MailFrom"];
            SmtpServer = new SmtpClient(ConfigurationManager.AppSettings["MailServer"]);
            Mail.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"]);
            Mail.Subject = ConfigurationManager.AppSettings["MailSubject"];
            string Password = ConfigurationManager.AppSettings["MailPassword"];
            if (!string.IsNullOrEmpty(Password))
            {
                SmtpServer.Credentials = new System.Net.NetworkCredential(mailFrom, Password);
            }
            Mail.IsBodyHtml = true;
        }
        void IEmailService.ConfigEmailService(string mailServer, string mailFrom, string mailSubject)
        {
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            SmtpClient SmtpServer = new SmtpClient(mailServer);
            Mail.From = new MailAddress(mailFrom);
            Mail.Subject = mailSubject;
            Mail.IsBodyHtml = true;
        }

    }
}