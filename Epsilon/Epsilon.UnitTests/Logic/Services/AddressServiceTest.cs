using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Resources.Logic.Address;
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
    public class AddressServiceTest
    {
        [Test]
        public async Task AddAddress_GlobalSwitchWorks()
        {
            var mockAddressServiceConfig = new Mock<IAddressServiceConfig>();
            mockAddressServiceConfig.Setup(x => x.GlobalSwitch_DisableAddAddress).Returns(true);
            var service = new AddressService(null, null, mockAddressServiceConfig.Object, null, null, null);

            var outcome = await service.AddAddress("some-user", "some-ip-address", null);

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected should be true.");
            Assert.AreEqual(AddressResources.GlobalSwitch_AddAddressDisabled_Message, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
            Assert.IsFalse(outcome.ReturnToForm, "Outcome field ReturnToForm should be false.");
        }
    }
}
