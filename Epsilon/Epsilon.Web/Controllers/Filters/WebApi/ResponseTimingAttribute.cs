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
using System.Diagnostics;
using System.Web.Http;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class ResponseTimingAttribute : ActionFilterAttribute
    {
        // NOTE: If you change the logic in this filter update
        // !!!!! the corresponding MVC filter as well. !!!!!!!

        private const string PROPERTIES_KEY = "Stopwatch";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var dbAppSettingsHelper =
                (IDbAppSettingsHelper)GlobalConfiguration.Configuration.DependencyResolver
                    .GetService(typeof(IDbAppSettingsHelper));

            if (dbAppSettingsHelper.GetBool(DbAppSettingKey.EnableResponseTiming) == true)
            {
                var stopwatch = new Stopwatch();
                actionContext.Request.Properties[PROPERTIES_KEY] = stopwatch;

                stopwatch.Start();
            }
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext.Request.Properties.ContainsKey(PROPERTIES_KEY))
            {
                var responseTimingService =
                    (IResponseTimingService)GlobalConfiguration.Configuration.DependencyResolver
                        .GetService(typeof(IResponseTimingService));

                var stopwatch = (Stopwatch)actionExecutedContext.Request.Properties[PROPERTIES_KEY];

                stopwatch.Stop();

                var timeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                var routeData = actionExecutedContext.Request.GetRouteData();
                string languageId = ((string)routeData.Values["languageId"]).ToLowerInvariant();
                string currentAction = ((string)routeData.Values["action"]).ToLowerInvariant();
                string currentController = ((string)routeData.Values["controller"]).ToLowerInvariant();
                string httpVerb = actionExecutedContext.Request.Method.Method.ToLowerInvariant();

                await responseTimingService.RecordAsync(languageId, currentController, currentAction, httpVerb, true, timeInMilliseconds);
            }
        }
    }
}