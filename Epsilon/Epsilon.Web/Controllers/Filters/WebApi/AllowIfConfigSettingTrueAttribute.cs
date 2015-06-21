using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Epsilon.Web.Controllers.Filters.WebApi
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

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // NOTE: If you change the logic in this filter update
            // !!!!! the corresponding MVC filter as well. !!!!!!!

            var notAllowed = (AppSettingsHelper.GetBool(_settingKey) != true);

            if (notAllowed)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }
}
