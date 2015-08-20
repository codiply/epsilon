﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Ninject;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DisableWholeWebsiteForMaintenanceAttribute : ActionFilterAttribute
    {
        public IDependencyResolver CurrentDependencyResolver
        {
            get { return GlobalConfiguration.Configuration.DependencyResolver; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding MVC filter as well. !!!!!!!

            var appSettingsHelper = (IAppSettingsHelper)CurrentDependencyResolver.GetService(typeof(IAppSettingsHelper));

            var notAllowed = (appSettingsHelper.GetBool(AppSettingsKey.DisableWholeWebsiteForMaintenance) == true);

            if (notAllowed)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}
