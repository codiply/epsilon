using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Common;
using Microsoft.AspNet.Identity;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class InternationalizationAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding WebApi filter as well. !!!!

            string languageId = string.Empty;

            // Attempt 1: For logged in user get it from UserPreference
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var userPreferenceService = CurrentDependencyResolver.GetService<IUserPreferenceService>();
                var userId = filterContext.HttpContext.User.Identity.GetUserId();
                var userPreference = userPreferenceService.Get(userId);
                if (userPreference != null)
                    languageId = userPreference.LanguageId;
            }

            // Attempt 2: Get it from the url
            if (string.IsNullOrWhiteSpace(languageId))
            {
                languageId = (string)filterContext.RouteData.Values["languageId"];
            }

            var dbAppSettingsHelper = CurrentDependencyResolver.GetService<IDbAppSettingsHelper>();

            // Attempt 3: Get it from the IP address country
            if (dbAppSettingsHelper.GetBool(DbAppSettingKey.GlobalSwitch_DisableUseOfGeoipInformation) != true
                && string.IsNullOrWhiteSpace(languageId))
            {
                var geoipInfoService = CurrentDependencyResolver.GetService<IGeoipInfoService>();
                var ipAddress = filterContext.HttpContext.GetSanitizedIpAddress();
                var geoip = geoipInfoService.GetInfo(ipAddress);
                if (geoip != null)
                {
                    var countryService = CurrentDependencyResolver.GetService<ICountryService>();
                    var country = countryService.GetCountry(geoip.CountryCode);
                    if (country != null)
                        languageId = country.MainLanguageId;
                }
            }

            var appSettingsHelper = CurrentDependencyResolver.GetService<IAppSettingsHelper>();

            // Attempt 4: Use the default
            if (string.IsNullOrEmpty(languageId))
                languageId = appSettingsHelper.GetString(AppSettingsKey.DefaultLanguageId);

            // Save back the languageId on the RouteData.
            filterContext.RouteData.Values["languageId"] = languageId;

            var languageService = DependencyResolver.Current.GetService<ILanguageService>();

            var language = languageService.GetLanguage(languageId);

            if (language == null || !language.IsAvailable)
            {
                var message = CommonResources.UnsupportedLanguageMessage;
                filterContext.Controller.ViewData.Model = message;
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }

            var cultureInfo = CultureInfo.GetCultureInfo(language.CultureCode);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}