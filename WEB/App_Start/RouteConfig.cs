using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WEB
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Search",
                url: "tim-kiem",
                defaults: new { controller = "Home", action = "Search" },
                  namespaces: new[] { "WEB.Controllers" }
            );

            //-------------------------------------------------------------

            // routes.MapRoute(
            //    name: "Home_Index",
            //    url: "{metatitle}-{id}",
            //    defaults: new { controller = "Home", action = "Index", metatitle = UrlParameter.Optional, id = UrlParameter.Optional },
            //    constraints: new { id = @"\d+" },
            //      namespaces: new[] { "WEB.Controllers" }
            //);

            // routes.MapRoute(
            //    name: "Home_Index_Detail",
            //    url: "{m_metatitle}-{m_id}/{metatitle}-{id}",
            //    defaults: new { controller = "Home", action = "Detail", metatitle = UrlParameter.Optional, id = UrlParameter.Optional, m_metatitle = UrlParameter.Optional, m_id = UrlParameter.Optional },
            //    constraints: new { id = @"\d+", m_id = @"\d+", },
            //      namespaces: new[] { "WEB.Controllers" }
            //);

            routes.MapRoute(
               name: "Home_Index",
               url: "{metatitle}",
               defaults: new { controller = "Home", action = "Index", metatitle = UrlParameter.Optional },
                 namespaces: new[] { "WEB.Controllers" }
           );

            routes.MapRoute(
               name: "Home_Index_Detail",
               url: "{m_metatitle}/{metatitle}",
               defaults: new { controller = "Home", action = "Detail", metatitle = UrlParameter.Optional,
                   m_metatitle = UrlParameter.Optional },
                 namespaces: new[] { "WEB.Controllers" }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                  namespaces: new[] { "WEB.Controllers" }
            );
        }
    }
}