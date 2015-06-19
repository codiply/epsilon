using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Ninject;
using Epsilon.Logic.Constants;
using Epsilon.Logic.SqlContext;
using System.Net;
using System.Threading.Tasks;
using Epsilon.Resources.Common;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Infrastructure;
using Epsilon.Logic.Services.Interfaces;

namespace Epsilon.Web.Controllers.Filters
{
    public class InternationalizationAttribute : ActionFilterAttribute
    {
        [Inject]
        public ILanguageService LanguageService { get; set; }

        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        [Inject]
        public IAppCache Cache { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string languageId = (string)filterContext.RouteData.Values["languageId"] 
                ?? AppSettingsHelper.GetString(AppSettingsKeys.DefaultLanguage);

            var language = Task.Run(async () => await LanguageService.GetLanguage(languageId)).Result;

            if (language == null || !language.IsAvailable)
            {
                var message = CommonResources.UnsupportedLanguage;
                filterContext.Result = new ContentResult { Content = message };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var cultureInfo = CultureInfo.GetCultureInfo(language.CultureCode);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}