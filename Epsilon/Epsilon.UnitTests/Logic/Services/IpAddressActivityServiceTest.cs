using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services;
using Moq;
using NUnit.Framework;
using System;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class IpAddressActivityServiceTest
    {
        [Test]
        public void RecordLogin_DoesNotThrowButLogsException()
        {
            var email = "test@test.com";
            var ipAddress = "1.2.3.4";

            Exception loggedException = null;
            var elmahHelper = CreateElmahHelper((ex) => loggedException = ex);
            var service = new IpAddressActivityService(null, elmahHelper);

            Assert.DoesNotThrow(async () => await service.RecordLogin(email, ipAddress),
                "Calling RecordLogin should not throw.");
            Assert.IsNotNull(loggedException,
                "The exception was not logged.");
        }

        [Test]
        public void RecordRegistration_DoesNotThrowButLogsException()
        {
            var userId = "user-id";
            var ipAddress = "1.2.3.4";

            Exception loggedException = null;
            var elmahHelper = CreateElmahHelper((ex) => loggedException = ex);
            var service = new IpAddressActivityService(null, elmahHelper);

            Assert.DoesNotThrow(async () => await service.RecordRegistration(userId, ipAddress),
                "Calling RecordRegistration should not throw.");
            Assert.IsNotNull(loggedException,
                "The exception was not logged.");
        }

        private IElmahHelper CreateElmahHelper(Action<Exception> elmahHelperRaiseHandler)
        {
            var mockElmahHelper = new Mock<IElmahHelper>();
            mockElmahHelper.Setup(x => x.Raise(It.IsAny<Exception>())).Callback(elmahHelperRaiseHandler);

            return mockElmahHelper.Object;
        }
    }
}
