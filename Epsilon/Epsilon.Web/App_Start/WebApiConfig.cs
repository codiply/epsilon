using Elmah.Contrib.WebApi;
using Epsilon.Logic.Constants;
using Epsilon.Web.Controllers.Handlers;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Epsilon.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable Elmah
            config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());

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
