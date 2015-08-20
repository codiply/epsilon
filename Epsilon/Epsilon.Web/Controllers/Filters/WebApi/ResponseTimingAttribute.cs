using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class ResponseTimingAttribute : ActionFilterAttribute
    {
        // NOTE: If you change the logic in this filter update
        // !!!!! the corresponding MVC filter as well. !!!!!!!

        private const string PROPERTIES_KEY = "Stopwatch";

        public IDependencyResolver CurrentDependencyResolver
        {
            get { return GlobalConfiguration.Configuration.DependencyResolver; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var dbAppSettingsHelper = (IDbAppSettingsHelper)CurrentDependencyResolver.GetService(typeof(IDbAppSettingsHelper));

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