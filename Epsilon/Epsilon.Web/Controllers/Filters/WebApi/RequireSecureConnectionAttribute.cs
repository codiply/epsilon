﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class RequireSecureConnectionAttribute : ActionFilterAttribute
    {
        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (AppSettingsHelper.GetBool(AppSettingsKey.DisableHttps) == true)
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