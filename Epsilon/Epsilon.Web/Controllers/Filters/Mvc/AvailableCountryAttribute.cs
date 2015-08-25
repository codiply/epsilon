using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using Epsilon.Logic.Services.Interfaces;
using System;
using System.Net;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AvailableCountryAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dbAppSettingsHelper = CurrentDependencyResolver.GetService<IDbAppSettingsHelper>();

            if (dbAppSettingsHelper.GetBool(DbAppSettingKey.GlobalSwitch_DisableUseOfGeoipInformation) == true)
                return;

            var geoipInfoService = CurrentDependencyResolver.GetService<IGeoipInfoService>();
            var countryService = CurrentDependencyResolver.GetService<ICountryService>();
            var ipAddress = filterContext.HttpContext.GetSanitizedIpAddress();
            var geoip = geoipInfoService.GetInfo(ipAddress);
            CountryId? countryId = geoip == null ? null : geoip.CountryCodeAsEnum();
            var isAvailable = countryId.HasValue && countryService.IsCountryAvailable(countryId.Value);

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
