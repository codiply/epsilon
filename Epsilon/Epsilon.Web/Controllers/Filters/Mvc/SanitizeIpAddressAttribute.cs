using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SanitizeIpAddressAttribute : ActionFilterAttribute
    {
        [Inject]
        public IIpAddressHelper IpAddressHelper { get; set; }

        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ipOverride = AppSettingsHelper.GetString(AppSettingsKey.IpAddressOverride);

            if (!string.IsNullOrWhiteSpace(ipOverride))
            {
                filterContext.HttpContext.SetSanitizedIpAddress(ipOverride);
                return;
            }

            if (filterContext != null && filterContext.HttpContext != null)
            {
                var ip = IpAddressHelper.GetClientIpAddress(filterContext.HttpContext.Request);
                filterContext.HttpContext.SetSanitizedIpAddress(ip);
            }
        }
    }
}