using Epsilon.Web.Controllers.BaseControllers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace Epsilon.UnitTests.Web.Controllers.BaseControllers
{
    [TestFixture]
    public class BaseApiControllerTest
    {
        [Test]
        public void AllApiControllers_ShouldDeriveFromBaseController()
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
            var message = new StringBuilder();
            if (failingApiControllers.Any())
            {
                message.Append(failingApiControllers.Count()).Append(" failing controller")
                    .Append(failingApiControllers.Count() == 1 ? ":" : "s:");
                foreach (var controller in failingApiControllers)
                {
                    message.Append(String.Format("\n{0}", controller.Name));
                }
            }

            Assert.IsFalse(failingApiControllers.Any(), message.ToString());
        }
    }
}
