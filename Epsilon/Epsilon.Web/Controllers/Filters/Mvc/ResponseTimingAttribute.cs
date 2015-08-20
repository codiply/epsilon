using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ResponseTimingAttribute : ActionFilterAttribute
    {
        // NOTE: If you change the logic in this filter update
        // !!!!! the corresponding WebApi filter as well. !!!!

        private const string ITEMS_KEY = "Stopwatch";

        public IDependencyResolver CurrentDependencyResolver
        {
            get { return DependencyResolver.Current; }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dbAppSettingsHelper = CurrentDependencyResolver.GetService<IDbAppSettingsHelper>();
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
                var responseTimingService = CurrentDependencyResolver.GetService<IResponseTimingService>();

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