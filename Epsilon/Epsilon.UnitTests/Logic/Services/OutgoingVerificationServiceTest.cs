using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Resources.Logic.OutgoingVerification;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class OutgoingVerificationServiceTest
    {
        [Test]
        public async Task Pick_GlobalSwitchWorks()
        {
            var mockOutgoingVerificationServiceConfig = new Mock<IOutgoingVerificationServiceConfig>();
            mockOutgoingVerificationServiceConfig.Setup(x => x.GlobalSwitch_DisablePickOutgoingVerification).Returns(true);
            var service = new OutgoingVerificationService(null, null, null, null, mockOutgoingVerificationServiceConfig.Object, null, null);

            var outcome = await service.Pick("some-user", "some-ip-address", Guid.NewGuid());

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected should be true.");
            Assert.AreEqual(OutgoingVerificationResources.GlobalSwitch_PickOutgoingVerificationDisabled_Message, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
        }
    }
}
