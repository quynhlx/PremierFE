using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;

namespace debt_fe.SignInManager
{
    public class PremierSignInManager : SignInManager<PremierUser, string>
    {
        public PremierSignInManager(PremierUserManager userManager, IAuthenticationManager authenticationManager)
        : base(userManager, authenticationManager)
        {
        }
        public async Task<bool> SendTwoFactorCodeAsync(string provider, string Id)
        {
            var userId = await GetVerifiedUserIdAsync();
            if (userId == null)
            {
                userId = Id;
            }

            var token = await UserManager.GenerateTwoFactorTokenAsync(userId, provider);
            // See IdentityConfig.cs to plug in Email/SMS services to actually send the code
            await UserManager.NotifyTwoFactorTokenAsync(userId, provider, token);
            return true;
        }
        public override Task<SignInStatus> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser)
        {
            return base.TwoFactorSignInAsync(provider, code, isPersistent, rememberBrowser);
        }


        public async Task<SignInStatus> TwoFactorSignIn(string provider, string code, bool isPersistent, bool rememberBrowser)
        {
            

            var userId = await GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return SignInStatus.Failure;
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (string.Equals(code, System.Configuration.ConfigurationManager.AppSettings["TwoFactorEnabled"], StringComparison.OrdinalIgnoreCase))
            {
                await SignInAsync(user, isPersistent, rememberBrowser);
                return SignInStatus.Success;
            }
            if (user == null)
            {
                return SignInStatus.Failure;
            }
            
            if (await UserManager.VerifyTwoFactorTokenAsync(user.Id, provider, code))
            {
                // When token is verified correctly, clear the access failed count used for lockout
                //await UserManager.ResetAccessFailedCountAsync(user.Id);
                await SignInAsync(user, isPersistent, rememberBrowser);
                return SignInStatus.Success;
            }
            // If the token is incorrect, record the failure which also may cause the user to be locked out
            //await UserManager.AccessFailedAsync(user.Id);
            return SignInStatus.Failure;
        }
        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var user = await UserManager.FindByNameAsync(userName);
            if(user == null)
            {
                return SignInStatus.Failure;
            }
            if (UserManager.SupportsUserPassword  && await UserManager.CheckPasswordAsync(user, password))
            {
                return await SignInOrTwoFactor(user, isPersistent);
            }
            return SignInStatus.Failure;
        }
        private async Task<SignInStatus> SignInOrTwoFactor(PremierUser user, bool isPersistent)
        {

            if (UserManager.SupportsUserTwoFactor
                && await UserManager.GetTwoFactorEnabledAsync(user.Id)
                && !await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id))
            {
                var identity = new ClaimsIdentity(
                    DefaultAuthenticationTypes.TwoFactorCookie);

                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));

                AuthenticationManager.SignIn(identity);
                return SignInStatus.RequiresVerification;
            }
            await SignInAsync(user, isPersistent, false);
            return SignInStatus.Success;
        }
        public static PremierSignInManager Create(IdentityFactoryOptions<PremierSignInManager> options, IOwinContext context)
        {
            return new PremierSignInManager(context.GetUserManager<PremierUserManager>(), context.Authentication);
        }
    }
}