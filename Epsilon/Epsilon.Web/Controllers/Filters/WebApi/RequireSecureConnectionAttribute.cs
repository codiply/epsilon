﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class RequireSecureConnectionAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            var appSettingsHelper = CurrentDependencyResolver.Resolve<IAppSettingsHelper>();

            if (appSettingsHelper.GetBool(AppSettingsKey.DisableHttps) == true)
            {
                return;
            }

            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }


}
