using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace debt_fe.Service
{
    public interface IEmailService
    {
        void SetBody(string serverPath, string templateName, System.Collections.Hashtable parameters, bool fromTemplate = true);
        void SetBody(string contentBody);
        void SendEmailTo(params string[] emails);
        void ConfigEmailService();
        void ConfigEmailService(string mailServer, string mailFrom, string mailSubject);
        void ConfigEmailService(string mailFrom, string password, string mailSubject = "Test Mail", string mailServer = "smtp.gmail.com");

    }
}