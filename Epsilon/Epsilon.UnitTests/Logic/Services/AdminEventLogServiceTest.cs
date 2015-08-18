using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class AdminEventLogServiceTest
    {
        [Test]
        public void Log_DoesNotThrowButLogsException()
        {
            var adminEventLogKey = AdminEventLogKey.GeoipClientFailure;


            Exception loggedException = null;
            var elmahHelper = CreateElmahHelper((ex) => loggedException = ex);
            var service = new AdminEventLogService(null, elmahHelper);

            Assert.DoesNotThrow(async () => await service.Log(adminEventLogKey, null),
                "Calling Log should not throw.");
            Assert.IsNotNull(loggedException, "The exception was not logged.");
        }

        private IElmahHelper CreateElmahHelper(Action<Exception> elmahHelperRaiseHandler)
        {
            var mockElmahHelper = new Mock<IElmahHelper>();
            mockElmahHelper.Setup(x => x.Raise(It.IsAny<Exception>())).Callback(elmahHelperRaiseHandler);

            return mockElmahHelper.Object;
        }
    }
}
