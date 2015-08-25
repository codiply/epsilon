using System.Web.Http.Dependencies;

namespace Epsilon.Logic.Infrastructure.Extensions
{
    public static class HttpDependencyResolverExtensions
    {
        public static T Resolve<T>(this IDependencyResolver dependencyResolver)
        {
            return (T)dependencyResolver.GetService(typeof(T));
        }

    }
}
