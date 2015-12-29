using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(debt_fe.Startup))]
namespace debt_fe
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier =
            System.Security.Claims.ClaimTypes.NameIdentifier;
        }
    }
}
