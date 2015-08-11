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
using Epsilon.Logic.Constants;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AvailableCountryAttribute : ActionFilterAttribute
    {
        [Inject]
        public IDbAppSettingsHelper DbAppSettingsHelper { get; set; }

        [Inject]
        public ICountryService CountryService { get; set; }

        [Inject]
        public IGeoipInfoService GeoipInfoService { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // TODO_PANOS_TEST
            if (DbAppSettingsHelper.GetBool(DbAppSettingKey.GlobalSwitch_DisableUseOfGeoipInformation) == true)
                return;

            var ipAddress = filterContext.HttpContext.GetSanitizedIpAddress();
            var geoip = GeoipInfoService.GetInfo(ipAddress);
            CountryId? countryId = geoip == null ? null : geoip.CountryCodeAsEnum();
            var isAvailable = countryId.HasValue && CountryService.IsCountryAvailable(countryId.Value);

            if (!isAvailable)
            {
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
