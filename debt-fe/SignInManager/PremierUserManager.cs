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
            this.PasswordHasher = new MD5PasswordHasher();
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
                if (String.Equals(user.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
                    return true;
                else
                    return false;
            });
        }
        public override Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            return Task.Run(async () =>
            {
                var TwoFactorEnabled = false;
                var user = await FindByIdAsync(userId);
                if (await IsPhoneNumberConfirmedAsync(userId) && user.TwoFactorEnabled )
                {
                    TwoFactorEnabled = true;
                }
                return TwoFactorEnabled;
            });

        }
        public override Task<string> GetPhoneNumberAsync(string userId)
        {
            return Task.Run(async () =>
            {
                var user = await Store.FindByIdAsync(userId);
                return user.Phone;
            });

        }
        public override bool SupportsUserTwoFactor
        {
            get
            {
                var SupportsUserTwoFactor = false;
                try
                {
                    SupportsUserTwoFactor = Convert.ToBoolean(ConfigurationManager.AppSettings["TwoFactorEnabled"]);
                }
                catch
                {

                }
                return SupportsUserTwoFactor;
            }
        }
        public override bool SupportsUserPassword
        {
            get
            {
                return true;
            }
        }
        public override Task<bool> IsPhoneNumberConfirmedAsync(string userId)
        {
            return Task.Run(async () =>
            {
                var IsPhoneNumberConfirmed = false;
                if (!string.IsNullOrEmpty(await GetPhoneNumberAsync(userId)))
                {
                    IsPhoneNumberConfirmed = true;
                }
                return IsPhoneNumberConfirmed;
            });
        }
    }
    public class MD5PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            //Check wether data was passed
            if ((password == null) || (password.Length == 0))
            {
                return String.Empty;
            }
            System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            var rs = String.Equals(hashedPassword, providedPassword, StringComparison.OrdinalIgnoreCase);
            if (rs)
                return PasswordVerificationResult.Success;
            else
                return PasswordVerificationResult.Failed;
        }
    }

}