﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DisableWholeWebsiteForMaintenanceAttribute : ActionFilterAttribute
    {
        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        public DisableWholeWebsiteForMaintenanceAttribute()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding WebApi filter as well. !!!!

            var notAllowed = (AppSettingsHelper.GetBool(AppSettingsKey.DisableWholeWebsiteForMaintenance) == true);

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