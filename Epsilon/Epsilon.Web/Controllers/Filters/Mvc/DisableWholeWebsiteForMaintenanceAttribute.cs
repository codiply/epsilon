using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.Common;
using System;
using System.Net;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DisableWholeWebsiteForMaintenanceAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding WebApi filter as well. !!!!

            var appSettingsHelper = CurrentDependencyResolver.GetService<IAppSettingsHelper>();

            var notAllowed = (appSettingsHelper.GetBool(AppSettingsKey.DisableWholeWebsiteForMaintenance) == true);

            if (notAllowed)
            {
                var message = CommonResources.ContentAccessDisallowedMessage;
                filterContext.Controller.ViewData.Model = message;
                filterContext.Result = new ViewResult
                {
                    ViewName = "WebsiteDisabledForMaintenance",
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            }
        }
    }
}
