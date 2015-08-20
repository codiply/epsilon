using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;

namespace Epsilon.Web.Controllers.Filters.WebApi
{
    public class BaseActionFilterAttribute : ActionFilterAttribute 
    {
        private IDependencyResolver _dependencyResolverOverride = null;

        public IDependencyResolver CurrentDependencyResolver
        {
            get
            {
                if (_dependencyResolverOverride == null)
                    return GlobalConfiguration.Configuration.DependencyResolver;
                else
                    return _dependencyResolverOverride;
            }

            set
            {
                _dependencyResolverOverride = value;
            }
        }
    }
}
