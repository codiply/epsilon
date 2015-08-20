using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using Ninject;
using System;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SanitizeIpAddressAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var appSettingsHelper = CurrentDependencyResolver.GetService<IAppSettingsHelper>();
            var ipOverride = appSettingsHelper.GetString(AppSettingsKey.IpAddressOverride);

            if (!string.IsNullOrWhiteSpace(ipOverride))
            {
                filterContext.HttpContext.SetSanitizedIpAddress(ipOverride);
                return;
            }

            if (filterContext != null && filterContext.HttpContext != null)
            {
                var ipAddressHelper = CurrentDependencyResolver.GetService<IIpAddressHelper>();
                var ip = ipAddressHelper.GetClientIpAddress(filterContext.HttpContext.Request);
                filterContext.HttpContext.SetSanitizedIpAddress(ip);
            }
        }
    }
}