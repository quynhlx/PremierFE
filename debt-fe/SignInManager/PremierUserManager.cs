using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;

namespace debt_fe.SignInManager
{
    public class PremierUserManager : UserManager<PremierUser>
    {
        public PremierUserManager(IUserStore<PremierUser> store)
            : base(store)
        {
        }
        public static PremierUserManager Create(IdentityFactoryOptions<PremierUserManager> options, IOwinContext context)
        {
            var manager = new PremierUserManager(new PremierUserStore());
            manager.UserValidator = new UserValidator<PremierUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 1,
                //RequireNonLetterOrDigit = true,
                //RequireDigit = true,
                //RequireLowercase = true,
                //RequireUppercase = true,
            };

            // Configure user lockout defaults
            //manager.UserLockoutEnabledByDefault = true;
            //manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<PremierUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<PremierUser>
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
                    new DataProtectorTokenProvider<PremierUser>(dataProtectionProvider.Create("Premier Identity"));
            }
            return manager;
        }


        public override Task<bool> CheckPasswordAsync(PremierUser user, string password)
        {
            return Task.Run(() =>
            {
                string passwordHash = this.PasswordHasher.HashPassword(password);
                if (user.PasswordHash == passwordHash)
                    return true;
                else
                    return false;
            });
        }

        public override Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            return Task.Run(() => {
                var TwoFactorEnabled = false;
                try
                {
                    TwoFactorEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["TwoFactorEnabled"]);
                }
                catch
                {

                }
                return TwoFactorEnabled;
            });
            
        }

        public override bool SupportsUserTwoFactor
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsUserPassword
        {
            get
            {
                return true;
            }
        }
    }

}