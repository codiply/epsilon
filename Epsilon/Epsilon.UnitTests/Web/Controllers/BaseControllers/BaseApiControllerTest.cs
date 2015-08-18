using Epsilon.Web.Controllers.BaseControllers;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Http;

namespace Epsilon.UnitTests.Web.Controllers.BaseControllers
{
    [TestFixture]
    public class BaseApiControllerTest
    {
        [Test]
        public void AllApiControllers_ShouldDeriveFromBaseApiController()
        {
            // Arrange
            var allApiControllerTypes =
                typeof(BaseApiController).Assembly.GetTypes()
                .Where(type => typeof(ApiController).IsAssignableFrom(type));

            // Act
            var failingApiControllers = allApiControllerTypes
                .Where(type => !typeof(BaseApiController).IsAssignableFrom(type))
                .ToList();

            // Assert
            var message = "";
            if (failingApiControllers.Any())
            {
                var sb = new StringBuilder();
                sb.Append("All Api Controllers should derive from BaseApiController!")
                    .Append(failingApiControllers.Count()).Append(" failing controller")
                    .Append(failingApiControllers.Count() == 1 ? ":" : "s:");
                foreach (var controller in failingApiControllers)
                {
                    sb.Append(string.Format("\n{0}", controller.Name));
                }
                message = sb.ToString();
            }

            Assert.IsFalse(failingApiControllers.Any(), message);
        }

        [Test]
        public void NoMvcFilter_ShouldBeUsedOnApiControllers()
        {
            var mvcFiltersNamespace = "Epsilon.Web.Controllers.Filters.Mvc";
            var allMvcFilterTypes =
                typeof(BaseApiController).Assembly.GetTypes()
                .Where(type => type.Namespace == mvcFiltersNamespace)
                .Where(type => !type.Name.Contains("<"))
                .ToList();

            Assert.IsTrue(allMvcFilterTypes.Any(),
                string.Format("No types found in namespace {0}. If namespace changed, please update test.", mvcFiltersNamespace));

            var failingControllers = typeof(BaseApiController).Assembly.GetTypes()
                .Where(controller => typeof(ApiController).IsAssignableFrom(controller))
                .SelectMany(controller => allMvcFilterTypes.Select(filter => new { Controller = controller, MvcFilter = filter }))
                .Where(x => x.Controller.GetCustomAttribute(x.MvcFilter) != null)
                .ToList();

            var failingActions = typeof(BaseApiController).Assembly.GetTypes()
                .Where(controller => typeof(ApiController).IsAssignableFrom(controller))
                .SelectMany(controller => controller.GetMethods())
                .SelectMany(action => allMvcFilterTypes.Select(filter => new { Action = action, MvcFilter = filter }))
                .Where(x => x.Action.GetCustomAttribute(x.MvcFilter, false) != null)
                .ToList();

            var message = "";
            if (failingControllers.Any())
            {
                var sb = new StringBuilder();
                sb.Append("No MVC filter should be used on Api Controllers!");
                sb.Append("\n")
                    .Append(failingControllers.Count()).Append(" failing controller")
                    .Append(failingControllers.Count() == 1 ? ":" : "s:");
                foreach (var controller in failingControllers)
                {
                    sb.Append(string.Format("\n{0} with filter {1}",
                        controller.Controller.FullName, controller.MvcFilter.FullName));
                }
                message = sb.ToString();
            }
            Assert.IsFalse(failingControllers.Any(), message);

            if (failingActions.Any())
            {
                var sb = new StringBuilder();
                sb.Append("No MVC filter should be used on Api Controllers Actions!");
                sb.Append("\n")
                    .Append(failingActions.Count()).Append(" failing action")
                    .Append(failingActions.Count() == 1 ? ":" : "s:");
                foreach (var action in failingActions)
                {
                    sb.Append(string.Format("\n{0} in {1} with filter {2}",
                        action.Action.Name, action.Action.DeclaringType.FullName, action.MvcFilter.FullName));
                }
                message = sb.ToString();
            }
            Assert.IsFalse(failingActions.Any(), message);
        }
    }
}
