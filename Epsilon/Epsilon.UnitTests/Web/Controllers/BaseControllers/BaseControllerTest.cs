using Epsilon.Web.Controllers.BaseControllers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Epsilon.UnitTests.Web.Controllers.BaseControllers
{
    public class BaseControllerTest
    {
        [Test]
        public void AllHttpPostControllerActionsShouldBeDecoratedWithValidateAntiForgeryTokenAttribute()
        {
            // Arrange
            var allControllerTypes =
                typeof(BaseController).Assembly.GetTypes()
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
            var message = new StringBuilder();
            if (failingActions.Any())
            {
                message.Append(failingActions.Count()).Append(" failing action")
                    .Append(failingActions.Count() == 1 ? ":" : "s:");
                foreach (var action in failingActions)
                {
                    message.Append(String.Format("\n{0} in {1}", action.Name, action.DeclaringType.Name));
                }
            }

            Assert.IsFalse(failingActions.Any(), message.ToString());
        }

        [Test]
        public void AllControllers_ShouldDeriveFromBaseController()
        {
            // Arrange
            var allControllerTypes =
                typeof(BaseController).Assembly.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type));

            // Act
            var failingControllers = allControllerTypes
                .Where(type => !typeof(BaseController).IsAssignableFrom(type))
                .ToList();

            // Assert
            var message = new StringBuilder();
            if (failingControllers.Any())
            {
                message.Append(failingControllers.Count()).Append(" failing controller")
                    .Append(failingControllers.Count() == 1 ? ":" : "s:");
                foreach (var controller in failingControllers)
                {
                    message.Append(String.Format("\n{0}", controller.Name));
                }
            }

            Assert.IsFalse(failingControllers.Any(), message.ToString());
        }
    }
}
