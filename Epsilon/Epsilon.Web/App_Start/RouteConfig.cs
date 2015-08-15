using System.Web.Mvc;
using System.Web.Routing;

namespace Epsilon.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{languageId}/{controller}/{action}/{id}",
                defaults: new
                {
                    languageId = "",
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                });
        }
    }
}
