using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AllowIfConfigSettingTrueAttribute : BaseActionFilterAttribute
    {
        private readonly string _settingKey;

        public AllowIfConfigSettingTrueAttribute(string settingKey)
        {
            _settingKey = settingKey;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding MVC filter as well. !!!!!!!

            var appSettingsHelper = CurrentDependencyResolver.Resolve<IAppSettingsHelper>();

            var notAllowed = (appSettingsHelper.GetBool(_settingKey) != true);

            if (notAllowed)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }
}
