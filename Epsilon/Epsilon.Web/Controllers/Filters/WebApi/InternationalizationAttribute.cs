﻿using Epsilon.Logic.Helpers.Interfaces;
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

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class InternationalizationAttribute : ActionFilterAttribute
    {
        [Inject]
        public ILanguageService LanguageService { get; set; }

        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        [Inject]
        public IAppCache Cache { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding MVC filter as well. !!!!!!!

            string languageId = (string)actionContext.RequestContext.RouteData.Values["languageId"] 
                ?? AppSettingsHelper.GetString(AppSettingsKeys.DefaultLanguageId);

            // I have to block because there are no asynchronous versions for MVC filters.
            // The languages are cached, so this should be quick.
            var language = LanguageService.GetLanguage(languageId);

            if (language == null || !language.IsAvailable)
            {
                actionContext.Response= new HttpResponseMessage(HttpStatusCode.NotFound);
                return;
            }

            var cultureInfo = CultureInfo.GetCultureInfo(language.CultureCode);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}