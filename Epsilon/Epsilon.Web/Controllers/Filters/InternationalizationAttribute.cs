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

namespace Epsilon.Web.Controllers.Filters
{
    public class InternationalizationAttribute : ActionFilterAttribute
    {
        [Inject]
        public IEpsilonContext DbContext { get; set; }

        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        [Inject]
        public IAppCache Cache { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string languageCode = (string)filterContext.RouteData.Values["language"] 
                ?? AppSettingsHelper.GetString(AppSettingsKeys.DefaultLanguage);
            string cultureCode = (string)filterContext.RouteData.Values["culture"]
                ?? AppSettingsHelper.GetString(AppSettingsKeys.DefaultCulture);

            var languageId = string.Format("{0}-{1}", languageCode, cultureCode);

            var language = Cache.Get(AppCacheKeys.Language(languageId), () => 
                DbContext.Languages
                .Include(x => x.UseLanguage)
                .SingleOrDefault(x => x.Id == languageId), WithLock.Yes);

            if (language == null)
            {
                var message = CommonResources.UnsupportedLanguage;
                filterContext.Result = new ContentResult { Content = message };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var cultureInfo = CultureInfo.GetCultureInfo(language.UseLanguageId ?? languageId);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}