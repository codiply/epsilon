using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.Data.Entity;
using Epsilon.Logic.Configuration.Interfaces;
using Moq;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Forms;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AddressServiceTest : BaseIntegrationTestWithRollback
    {

        private void SetupConfig(IKernel container, int searchAddressResultsLimit)
        {
            var mockConfig = new Mock<IAddressServiceConfig>();
            mockConfig.Setup(x => x.SearchAddressResultsLimit).Returns(searchAddressResultsLimit);

            container.Rebind<IAddressServiceConfig>().ToConstant(mockConfig.Object);
        }

        private void SetupAntiAbuseServiceResponse(IKernel container, AntiAbuseServiceResponse response)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();
            mockAntiAbuseService.Setup(x => x.CanAddAddress(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(response));

            container.Rebind<IAntiAbuseService>().ToConstant(mockAntiAbuseService.Object);
        }

        private void SetupAddressVerficationServiceResponse(IKernel container, AddressVerificationResponse response)
        {
            var mockAddressVerificationService = new Mock<IAddressVerificationService>();
            mockAddressVerificationService.Setup(x => x.Verify(It.IsAny<AddressForm>()))
                .Returns(Task.FromResult(response));

            container.Rebind<IAddressVerificationService>().ToConstant(mockAddressVerificationService.Object);
        }
    }
}
