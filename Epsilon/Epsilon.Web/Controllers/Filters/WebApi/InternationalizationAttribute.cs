using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using Ninject;
using Epsilon.Logic.Constants;
using Epsilon.Logic.SqlContext;
using System.Net;
using System.Threading.Tasks;
using Epsilon.Resources.Common;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Infrastructure;
using Epsilon.Logic.Services.Interfaces;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Web.Http;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class InternationalizationAttribute : ActionFilterAttribute
    {
        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        [Inject]
        public IAppCache Cache { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding MVC filter as well. !!!!!!!

            // NOTE: I assume the languageId is always defined for WebApi requests. If not I fall back to default language.

            string languageId = (string)actionContext.RequestContext.RouteData.Values["languageId"] 
                ?? AppSettingsHelper.GetString(AppSettingsKey.DefaultLanguageId);
            
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