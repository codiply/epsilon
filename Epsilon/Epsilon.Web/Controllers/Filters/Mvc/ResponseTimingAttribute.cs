﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    public class ResponseTimingAttribute : ActionFilterAttribute
    {
        // NOTE: If you change the logic in this filter update
        // !!!!! the corresponding WebApi filter as well. !!!!

        private const string ITEMS_KEY = "Stopwatch";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dbAppSettingsHelper = DependencyResolver.Current.GetService<IDbAppSettingsHelper>();
            if (dbAppSettingsHelper.GetBool(DbAppSettingKey.EnableResponseTiming) == true)
            {
                var stopwatch = new Stopwatch();
                filterContext.HttpContext.Items[ITEMS_KEY] = stopwatch;

                stopwatch.Start();
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.HttpContext.Items.Contains(ITEMS_KEY))
            {
                var responseTimingService = DependencyResolver.Current.GetService<IResponseTimingService>();

                var stopwatch = (Stopwatch)filterContext.HttpContext.Items[ITEMS_KEY];

                stopwatch.Stop();

                var timeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                var httpContext = filterContext.HttpContext;

                var routeData = httpContext.Request.RequestContext.RouteData;
                string languageId = routeData.GetRequiredString("languageId").ToLowerInvariant();
                string currentAction = routeData.GetRequiredString("action").ToLowerInvariant();
                string currentController = routeData.GetRequiredString("controller").ToLowerInvariant();
                string httpVerb = httpContext.Request.HttpMethod.ToLowerInvariant();

                responseTimingService.Record(languageId, currentController, currentAction, httpVerb, false, timeInMilliseconds);
            }
        }
    }
}