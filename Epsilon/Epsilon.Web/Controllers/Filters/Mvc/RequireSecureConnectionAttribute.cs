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
        public IDependencyResolver CurrentDependencyResolver
        {
            get { return DependencyResolver.Current; }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            var appSettingsHelper = CurrentDependencyResolver.GetService<IAppSettingsHelper>();

            if (appSettingsHelper.GetBool(AppSettingsKey.DisableHttps) == true)
            {
                return;
            }

            base.OnAuthorization(filterContext);
        }
    }
}