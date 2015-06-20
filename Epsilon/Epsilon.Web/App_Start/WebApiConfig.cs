using Epsilon.Logic.Constants;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace Epsilon.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{languageId}/api/{controller}/{id}",
                defaults: new
                {
                    languageId = ConfigurationManager.AppSettings.Get(AppSettingsKeys.DefaultLanguageId),
                    id = RouteParameter.Optional
                }
            );
        }
    }
}
