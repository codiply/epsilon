﻿using Epsilon.Logic.Helpers.Interfaces;
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

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class InternationalizationAttribute : ActionFilterAttribute
    {
        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        [Inject]
        public IAppCache Cache { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding WebApi filter as well. !!!!

            string languageId = (string)filterContext.RouteData.Values["languageId"] 
                ?? AppSettingsHelper.GetString(AppSettingsKey.DefaultLanguageId);

            var languageService = DependencyResolver.Current.GetService<ILanguageService>();

            var language = languageService.GetLanguage(languageId);

            if (language == null || !language.IsAvailable)
            {
                var message = CommonResources.UnsupportedLanguageMessage;
                filterContext.Controller.ViewData.Model = message;
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var cultureInfo = CultureInfo.GetCultureInfo(language.CultureCode);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}