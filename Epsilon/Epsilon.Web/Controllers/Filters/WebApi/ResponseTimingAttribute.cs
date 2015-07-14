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

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class ResponseTimingAttribute : ActionFilterAttribute
    {
        // NOTE: If you change the logic in this filter update
        // !!!!! the corresponding MVC filter as well. !!!!!!!

        [Inject]
        public IResponseTimingService ResponseTimingService { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var stopwatch = new Stopwatch();
            actionContext.Request.Properties["Stopwatch"] = stopwatch;

            stopwatch.Start();
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var stopwatch = (Stopwatch)actionExecutedContext.Request.Properties["Stopwatch"];
            stopwatch.Stop();

            var timeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            var routeData = actionExecutedContext.Request.GetRouteData();
            string currentAction = (string)routeData.Values["action"];
            string currentController = (string)routeData.Values["controller"];
            string httpVerb = actionExecutedContext.Request.Method.Method;

            await ResponseTimingService.RecordAsync(currentController, currentAction, httpVerb, true, timeInMilliseconds);
        }
    }
}