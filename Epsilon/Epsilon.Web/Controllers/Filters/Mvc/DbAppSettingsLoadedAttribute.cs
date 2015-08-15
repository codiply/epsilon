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
    public class DbAppSettingsLoadedAttribute : ActionFilterAttribute
    {
        [Inject]
        public IDbAppSettingsHelper DbAppSettingsHelper { get; set; }

        [Inject]
        public IAdminAlertService AdminAlertService { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // TODO_TEST_PANOS
            if (DbAppSettingsHelper.GetBool(DbAppSettingKey.AlwaysTrue) == true)
                return;

            AdminAlertService.SendAlert(AdminAlertKey.DbAppSettingsNotLoaded, doNotUseDatabase: true);

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
