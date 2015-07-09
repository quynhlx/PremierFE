using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace debt_fe
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
/*
            List<Claim> _claims = new List<Claim>();
_claims.AddRange(new List<Claim>
{
    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "")),
    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "")
});*/
        }
    }
}
