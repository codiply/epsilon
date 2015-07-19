using Epsilon.Web.Controllers.BaseControllers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Reflection;

namespace Epsilon.UnitTests.Web.Controllers.BaseControllers
{
    public class BaseMvcControllerTest
    {
        [Test]
        public void AllHttpPostControllerActionsShouldBeDecoratedWithValidateAntiForgeryTokenAttribute()
        {
            // Arrange
            var allControllerTypes =
                typeof(BaseMvcController).Assembly.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type));
            var allControllerActions = allControllerTypes.SelectMany(type => type.GetMethods());

            // Act
            var failingActions = allControllerActions
                .Where(method =>
                    Attribute.GetCustomAttribute(method, typeof(HttpPostAttribute)) != null)
                .Where(method =>
                    Attribute.GetCustomAttribute(method, typeof(ValidateAntiForgeryTokenAttribute)) == null)
                .ToList();

            // Assert
            var message = "";
            if (failingActions.Any())
            {
                var sb = new StringBuilder();
                sb.Append("All HttpPost actions should be decorated with [ValidateAntiForgeryToken]!\n")
                    .Append(failingActions.Count()).Append(" failing action")
                    .Append(failingActions.Count() == 1 ? ":" : "s:");
                foreach (var action in failingActions)
                {
                    sb.Append(string.Format("\n{0} in {1}", action.Name, action.DeclaringType.Name));
                }
                message = sb.ToString();
            }

            Assert.IsFalse(failingActions.Any(), message);
        }

        [Test]
        public void AllMvcControllers_ShouldDeriveFromBaseController()
        {
            // Arrange
            var allControllerTypes =
                typeof(BaseMvcController).Assembly.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type));

            // Act
            var failingControllers = allControllerTypes
                .Where(type => !typeof(BaseMvcController).IsAssignableFrom(type))
                .ToList();

            // Assert
            var message = "";
            if (failingControllers.Any())
            {
                var sb = new StringBuilder();
                sb.Append("All MVC Controllers should derive from BaseMvcController.")
                    .Append(failingControllers.Count()).Append(" failing controller")
                    .Append(failingControllers.Count() == 1 ? ":" : "s:");
                foreach (var controller in failingControllers)
                {
                    sb.Append(string.Format("\n{0}", controller.Name));
                }
                message = sb.ToString();
            }

            Assert.IsFalse(failingControllers.Any(), message);
        }

        [Test]
        public void NoApiFilter_ShouldBeUsedOnMvcControllers()
        {
            var apiFiltersNamespace = "Epsilon.Web.Controllers.Filters.WebApi";
            var allApiFilterTypes =
                typeof(BaseApiController).Assembly.GetTypes()
                .Where(type => type.Namespace == apiFiltersNamespace)
                // Extra filters appear in this list when using asynchronous filters.
                .Where(type => !type.FullName.Contains("+"))
                .ToList();

            Assert.IsTrue(allApiFilterTypes.Any(),
                string.Format("No types found in namespace {0}. If namespace changed, please update test.", apiFiltersNamespace));

            var failingControllers = typeof(BaseMvcController).Assembly.GetTypes()
                .Where(controller => typeof(Controller).IsAssignableFrom(controller))
                .SelectMany(controller => allApiFilterTypes.Select(filter => new { Controller = controller, ApiFilter = filter }))
                .Where(x => x.Controller.GetCustomAttribute(x.ApiFilter) != null)
                .ToList();

            var failingActions = typeof(BaseMvcController).Assembly.GetTypes()
                .Where(controller => typeof(Controller).IsAssignableFrom(controller))
                .SelectMany(controller => controller.GetMethods())
                .SelectMany(action => allApiFilterTypes.Select(filter => new { Action = action, ApiFilter = filter }))
                .Where(x => x.Action.GetCustomAttribute(x.ApiFilter, false) != null)
                .ToList();

            var message = "";
            if (failingControllers.Any())
            {
                var sb = new StringBuilder();
                sb.Append("No Api filter should be used on MVC Controllers!");
                sb.Append("\n")
                    .Append(failingControllers.Count()).Append(" failing controller")
                    .Append(failingControllers.Count() == 1 ? ":" : "s:");
                foreach (var controller in failingControllers)
                {
                    sb.Append(string.Format("\n{0} with filter {1}",
                        controller.Controller.FullName, controller.ApiFilter.FullName));
                }
                message = sb.ToString();
            }
            Assert.IsFalse(failingControllers.Any(), message);

            if (failingActions.Any())
            {
                var sb = new StringBuilder();
                sb.Append("No Api filter should be used on MVC Controllers Actions!");
                sb.Append("\n")
                    .Append(failingActions.Count()).Append(" failing action")
                    .Append(failingActions.Count() == 1 ? ":" : "s:");
                foreach (var action in failingActions)
                {
                    sb.Append(string.Format("\n{0} in {1} with filter {2}",
                        action.Action.Name, action.Action.DeclaringType.FullName, action.ApiFilter.FullName));
                }
                message = sb.ToString();
            }
            Assert.IsFalse(failingActions.Any(), message);
        }
    }
}
