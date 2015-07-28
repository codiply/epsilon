using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Resources.Logic.AntiAbuse;
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
    public class AntiAbuseServiceTest
    {
        [Test]
        public async Task CanRegister_GlobalSwitchWorks()
        {
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();
            mockAntiAbuseServiceConfig.Setup(x => x.GlobalSwitch_DisableRegister).Returns(true);
            var service = new AntiAbuseService(null, mockAntiAbuseServiceConfig.Object, null, null, null);

            var outcome = await service.CanRegister("some-ip-address");

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected should be true.");
            Assert.AreEqual(AntiAbuseResources.GlobalSwitch_RegisterDisabled_Message, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
        }
    }
}
