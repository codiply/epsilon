using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using Epsilon.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AvailableCountryAttribute : BaseActionFilterAttribute
    {
        private readonly IList<string> _crawlerCountryWhitelist;

        public AvailableCountryAttribute()
        {
            var appSettingsHelper = CurrentDependencyResolver.GetService<IAppSettingsHelper>();
            var countryListString = appSettingsHelper.GetString(AppSettingsKey.CrawlerCountryWhitelist);
            _crawlerCountryWhitelist = 
                countryListString.Split(',', ';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant()).ToList();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dbAppSettingsHelper = CurrentDependencyResolver.GetService<IDbAppSettingsHelper>();

            if (dbAppSettingsHelper.GetBool(DbAppSettingKey.GlobalSwitch_DisableUseOfGeoipInformation) == true)
                return;

            var geoipInfoService = CurrentDependencyResolver.GetService<IGeoipInfoService>();
            var countryService = CurrentDependencyResolver.GetService<ICountryService>();
            var ipAddress = filterContext.HttpContext.GetSanitizedIpAddress();
            var geoip = geoipInfoService.GetInfo(ipAddress);

            // I allow crawlers from whitelisted countries.
            if (filterContext.HttpContext.Request.Browser.Crawler && 
                _crawlerCountryWhitelist.Contains(geoip.CountryCode.ToLowerInvariant()))
                return;

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
