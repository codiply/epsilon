using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Ninject;
using System;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class RequireSecureConnectionAttribute : RequireHttpsAttribute
    {
        [Inject]
        public IAppSettingsHelper AppSettingsHelper { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (AppSettingsHelper.GetBool(AppSettingsKey.DisableHttps) == true)
            {
                return;
            }

            base.OnAuthorization(filterContext);
        }
    }
}