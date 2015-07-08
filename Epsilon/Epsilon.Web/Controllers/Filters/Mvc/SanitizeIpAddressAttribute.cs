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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SanitizeIpAddressAttribute : ActionFilterAttribute
    {
        [Inject]
        public IIpAddressHelper Helper { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null)
            {
                var ip = Helper.GetClientIpAddress(filterContext.HttpContext.Request);
                filterContext.HttpContext.SetSanitizedIpAddress(ip);
            }
        }
    }
}