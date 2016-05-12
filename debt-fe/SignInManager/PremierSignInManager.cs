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
            var id = Convert.ToString(user.Id);

            if (UserManager.SupportsUserTwoFactor
                && await UserManager.GetTwoFactorEnabledAsync(user.Id)
                && !await AuthenticationManager.TwoFactorBrowserRememberedAsync(id))
            {
                var identity = new ClaimsIdentity(
                    DefaultAuthenticationTypes.TwoFactorCookie);

                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));

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