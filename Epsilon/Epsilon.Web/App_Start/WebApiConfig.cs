using Epsilon.Logic.Constants;
using Epsilon.Web.Controllers.Handlers;
using System.Configuration;
using System.Web.Http;

namespace Epsilon.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.MessageHandlers.Add(new EnforceHttpsHandler());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{languageId}/api/{controller}/{action}/{id}",
                defaults: new
                {
                    languageId = ConfigurationManager.AppSettings.Get(AppSettingsKey.DefaultLanguageId),
                    id = RouteParameter.Optional
                }
            );
        }
    }
}
