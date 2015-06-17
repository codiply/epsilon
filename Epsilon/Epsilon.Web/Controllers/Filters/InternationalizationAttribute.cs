using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters
{
    public class InternationalizationAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            string language = (string)filterContext.RouteData.Values["language"] ?? "en";
            string culture = (string)filterContext.RouteData.Values["culture"] ?? "GB";

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(string.Format("{0}-{1}", language, culture));
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(string.Format("{0}-{1}", language, culture));

        }
    }
}