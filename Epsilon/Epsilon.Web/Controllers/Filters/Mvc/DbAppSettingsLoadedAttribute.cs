using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Common;
using Ninject;
using System;
using System.Net;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DbAppSettingsLoadedAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dbAppSettingsHelper = CurrentDependencyResolver.GetService<IDbAppSettingsHelper>();

            if (dbAppSettingsHelper.GetBool(DbAppSettingKey.AlwaysTrue) == true)
                return;

            var adminAlertService = CurrentDependencyResolver.GetService<IAdminAlertService>();

            adminAlertService.SendAlert(AdminAlertKey.DbAppSettingsNotLoaded, doNotUseDatabase: true);

            var message = CommonResources.WebsiteDownMessage;
            filterContext.Controller.ViewData.Model = message;
            filterContext.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = filterContext.Controller.ViewData,
                TempData = filterContext.Controller.TempData
            };
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}
