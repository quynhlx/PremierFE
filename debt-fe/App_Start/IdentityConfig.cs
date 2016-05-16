using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using debt_fe.Models;
using System.Net.Mail;
using System.Net.Mime;
using System.Configuration;
using System.Net;
using System.Text;
using System.IO;

namespace debt_fe
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            string mailFrom = ConfigurationManager.AppSettings["MailFrom"];
            string mailServer = ConfigurationManager.AppSettings["MailServer"];
            string mailPassword = ConfigurationManager.AppSettings["MailPassword"];
            // Plug in your email service here to send an email.
            string text = message.Body;
            string html = message.Body;
            //do whatever you want to the message        
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(mailFrom);
            msg.To.Add(new MailAddress(message.Destination));
            msg.Subject = message.Subject;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient(mailServer, Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(mailFrom, mailPassword);
            smtpClient.Credentials = credentials;
            smtpClient.Send(msg);
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            string numberPhone = message.Destination;
            // Plug in your SMS service here to send a text message.
            var inboundDID = ConfigurationManager.AppSettings["PhoneDealer"].ToString();
            var emailSMS = ConfigurationManager.AppSettings["EmailSMS"].ToString();
            debt_fe.SMSService.WSAgentSoapClient smsService = new SMSService.WSAgentSoapClient("WSAgentSoap12");
            //var rs = smsService.SendSMSExt(usernameSMS, passwordSMS, DateTime.Now.ToString("yyyyMMddHHmm"), DateTime.Now.ToString("yyyyMMddHHmm"), string.Empty, "+841688166199", 1, Message, emailSMS, string.Empty, 1, string.Empty);
            var rs = SendSMS_Nexmo(inboundDID, numberPhone, message.Body);
            return Task.FromResult(0);
        }
        public string SendSMS_Nexmo(string strPhoneFrom, string strPhoneTo, string strMessage)
        {

            var sReturn = string.Empty;
            try
            {
                string sURL = ConfigurationManager.AppSettings["PhoneAPI_Url"];
                string content = "api_key=" + ConfigurationManager.AppSettings["PhoneAPI_AppKey"] + "&api_secret=" + ConfigurationManager.AppSettings["PhoneAPI_AppPass"];
                content += "&to=" + strPhoneTo + "&from=" + "1" + strPhoneFrom;
                content += "&text=" + strMessage;
                content += "&callback=" + ConfigurationManager.AppSettings["PhoneAPI_UrlCallback"];
             
                var request = (HttpWebRequest)WebRequest.Create(sURL);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] _byteVersion = Encoding.UTF8.GetBytes(content);
                request.ContentLength = _byteVersion.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(_byteVersion, 0, _byteVersion.Length);
                stream.Close();
                var response = (HttpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    sReturn = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                sReturn = ex.Message;
            }
            return sReturn;
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
