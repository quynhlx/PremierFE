﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace debt_fe
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
           
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Document", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "SignatureDownload2",
               url: "{controller}/{action}/{token}/{docId}",
               defaults: new { controller = "Document", action = "SignatureDownload2", token = UrlParameter.Optional, docId = UrlParameter.Optional }
           );

        }
    }
}
