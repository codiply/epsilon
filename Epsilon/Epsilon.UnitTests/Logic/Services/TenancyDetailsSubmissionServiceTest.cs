using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Resources.Logic.TenancyDetailsSubmission;
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
    public class TenancyDetailsSubmissionServiceTest
    {
        [Test]
        public async Task Create_GlobalSwitchWorks()
        {
            var mockTenancyDetailsSubmissionServiceConfig = new Mock<ITenancyDetailsSubmissionServiceConfig>();
            mockTenancyDetailsSubmissionServiceConfig.Setup(x => x.GlobalSwitch_DisableCreateTenancyDetailsSubmission).Returns(true);
            var service = new TenancyDetailsSubmissionService(null, null, mockTenancyDetailsSubmissionServiceConfig.Object, null, null, null, null, null);

            var outcome = await service.Create("some-user", "some-ip-address", Guid.NewGuid(), Guid.NewGuid());

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected should be true.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.GlobalSwitch_CreateTenancyDetailsSubmissionDisabled_Message, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
            Assert.IsFalse(outcome.ReturnToForm, "Outcome field ReturnToForm should be false.");
        }
    }
}
