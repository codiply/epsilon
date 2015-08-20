using Epsilon.Logic.Constants;
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
    public class RequireSecureConnectionAttribute : ActionFilterAttribute
    {
        public IDependencyResolver CurrentDependencyResolver
        {
            get { return GlobalConfiguration.Configuration.DependencyResolver; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            var appSettingsHelper = (IAppSettingsHelper)CurrentDependencyResolver.GetService(typeof(IAppSettingsHelper));

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
