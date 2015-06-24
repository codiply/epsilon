using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AllowIfConfigSettingTrueAttribute : ActionFilterAttribute
    {
        private readonly string _settingKey;

        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        public AllowIfConfigSettingTrueAttribute(string settingKey)
        {
            _settingKey = settingKey;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding WebApi filter as well. !!!!

            var notAllowed = (AppSettingsHelper.GetBool(_settingKey) != true);

            if (notAllowed)
            {
                var message = CommonResources.ContentAccessDisallowedMessage;
                filterContext.Result = new ContentResult { Content = message };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
