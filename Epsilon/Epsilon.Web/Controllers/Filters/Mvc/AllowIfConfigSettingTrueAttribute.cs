using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.Common;
using System;
using System.Net;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AllowIfConfigSettingTrueAttribute : BaseActionFilterAttribute
    {
        private readonly string _settingKey;

        public AllowIfConfigSettingTrueAttribute(string settingKey)
        {
            _settingKey = settingKey;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding WebApi filter as well. !!!!

            var appSettingsHelper = CurrentDependencyResolver.GetService<IAppSettingsHelper>();

            var notAllowed = (appSettingsHelper.GetBool(_settingKey) != true);

            if (notAllowed)
            {
                var message = CommonResources.ContentAccessDisallowedMessage;
                filterContext.Controller.ViewData.Model = message;
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
