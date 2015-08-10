using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Epsilon.Logic.Infrastructure.Extensions;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AvailableCountryAttribute : ActionFilterAttribute
    {
        private readonly string _settingKey;

        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        [Inject]
        public ICountryService CountryService { get; set; }

        [Inject]
        public IGeoipInfoService GeoipInfoService { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var ipAddress = filterContext.HttpContext.GetSanitizedIpAddress();
            var geoip = GeoipInfoService.GetInfo(ipAddress);
            CountryId? countryId = geoip == null ? null : geoip.CountryCodeAsEnum();
            var isAvailable = countryId.HasValue && CountryService.IsCountryAvailable(countryId.Value);

            if (!isAvailable)
            {
                var message = CommonResources.ServiceNotAvailableInYourCountryMessage;
                filterContext.Result = new ViewResult
                {
                    ViewName = "UnavailableCountry",
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            }
        }
    }
}
