using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Resources.Logic.PropertyInfoAccess;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class PropertyInfoAccessServiceTest
    {
        [Test]
        public async Task Create_GlobalSwitchWorks()
        {
            var mockOutgoingVerificationServiceConfig = new Mock<IPropertyInfoAccessServiceConfig>();
            mockOutgoingVerificationServiceConfig.Setup(x => x.GlobalSwitch_DisableCreatePropertyInfoAccess).Returns(true);
            var service = new PropertyInfoAccessService(mockOutgoingVerificationServiceConfig.Object, null, null, null, null, null, null, null);

            var outcome = await service.Create("some-user", "some-ip-address", Guid.NewGuid(), Guid.NewGuid());

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected should be true.");
            Assert.AreEqual(PropertyInfoAccessResources.GlobalSwitch_CreatePropertyInfoAccessDisabled_Message, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
        }
    }
}
