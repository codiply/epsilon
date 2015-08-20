using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class InternationalizationAttribute : ActionFilterAttribute
    {
        public IDependencyResolver CurrentDependencyResolver
        {
            get { return GlobalConfiguration.Configuration.DependencyResolver; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding MVC filter as well. !!!!!!!

            // NOTE: I assume the languageId is always defined for WebApi requests. If not I fall back to default language.

            var appSettingsHelper = (IAppSettingsHelper)CurrentDependencyResolver.GetService(typeof(IAppSettingsHelper));

            string languageId = (string)actionContext.RequestContext.RouteData.Values["languageId"] 
                ?? appSettingsHelper.GetString(AppSettingsKey.DefaultLanguageId);
            
            var languageService = 
                (ILanguageService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILanguageService));

            var language = languageService.GetLanguage(languageId);

            if (language == null || !language.IsAvailable)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
                return;
            }

            var cultureInfo = CultureInfo.GetCultureInfo(language.CultureCode);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}