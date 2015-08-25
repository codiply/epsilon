using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using Epsilon.Logic.Services.Interfaces;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class InternationalizationAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding MVC filter as well. !!!!!!!

            // NOTE: I assume the languageId is always defined for WebApi requests. If not I fall back to default language.

            var appSettingsHelper = CurrentDependencyResolver.Resolve<IAppSettingsHelper>();

            string languageId = (string)actionContext.RequestContext.RouteData.Values["languageId"] 
                ?? appSettingsHelper.GetString(AppSettingsKey.DefaultLanguageId);
            
            var languageService = 
                CurrentDependencyResolver.Resolve<ILanguageService>();

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