using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    public class BaseActionFilterAttribute : ActionFilterAttribute
    {
        private IDependencyResolver _dependencyResolverOverride = null;

        public IDependencyResolver CurrentDependencyResolver
        {
            get
            {
                if (_dependencyResolverOverride == null)
                    return DependencyResolver.Current;
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