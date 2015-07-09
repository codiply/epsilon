using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Logic.AntiAbuse;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AntiAbuseServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task CanRegister_WithAllChecksDisabled_ReturnsIsRejectedFalse()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var container = CreateContainer();
            SetupContainerForCanRegister(container,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var service = container.Get<IAntiAbuseService>();

            for (int i = 0; i < 10; i++)
            {
                var user = await CreateUser(container, String.Format("test{0}@test.com", i), ipAddress);
            }

            var response = await service.CanRegister(ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
        }

        [Test]
        public async Task CanRegister_CheckGlobalFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = "2/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";
            
            var containerUnderTest = CreateContainer();
            SetupContainerForCanRegister(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            string adminAlertKey = string.Empty;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            // I create the first user.
            var user1 = await CreateUser(helperContainer, "test1@test.com", "1.2.3.1");

            var firstResponse = await serviceUnderTest.CanRegister("1.2.3.2");
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");
            Assert.AreEqual(string.Empty, adminAlertKey, "Admin alert should not be sent the first time.");

            // I create the second user
            var user2 = await CreateUser(helperContainer, "test2@test.com", "1.2.3.3");

            var secondResponse = await serviceUnderTest.CanRegister("1.2.3.4");
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.Register_GlobalFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.AreEqual(AdminAlertKey.RegistrationGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the second time.");
        }

        [Test]
        public async Task CanRegister_CheckGlobalFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = String.Format("1/{0}S", periodInSeconds);
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanRegister(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            string adminAlertKey = string.Empty;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", "1.2.3.4");

            var firstResponse = await serviceUnderTest.CanRegister("1.2.3.4");
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.Register_GlobalFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.AreEqual(AdminAlertKey.RegistrationGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the second time.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanRegister("2.3.4.5");
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanRegister_CheckIpAddressFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanRegister(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            // I create the first user.
            var user1 = await CreateUser(helperContainer, "test1@test.com", ipAddress);

            var firstResponse = await serviceUnderTest.CanRegister(ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I create the second user
            var user2 = await CreateUser(helperContainer, "test2@test.com", ipAddress);

            var secondResponse = await serviceUnderTest.CanRegister(ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.Register_IpAddressFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanRegister_CheckIpAddressFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "1/1D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = String.Format("1/{0}S", periodInSeconds);
            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanRegister(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var firstResponse = await serviceUnderTest.CanRegister(ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.Register_IpAddressFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanRegister(ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        private static void SetupContainerForCanRegister(IKernel container,
            bool disableGlobalFrequencyCheck, string globalMaxFrequency,
            bool disableIpAddressFrequencyCheck, string maxFrequencyPerIpAddress)
        {
            var parseHelper = new ParseHelper();
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.Register_DisableGlobalFrequencyCheck)
                .Returns(disableGlobalFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.Register_GlobalMaxFrequency)
                .Returns(parseHelper.ParseFrequency(globalMaxFrequency));
            mockAntiAbuseServiceConfig.Setup(x => x.Register_DisableIpAddressFrequencyCheck)
                .Returns(disableIpAddressFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.Register_MaxFrequencyPerIpAddress)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerIpAddress));
            container.Rebind<IAntiAbuseServiceConfig>().ToConstant(mockAntiAbuseServiceConfig.Object);
        }

        private static void SetupContainerForCanAddAddress(IKernel container,
            bool disableUserFrequencyCheck, string maxFrequencyPerUser,
            bool disableIpAddressFrequencyCheck, string maxFrequencyPerIpAddress)

        {
            var parseHelper = new ParseHelper();
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableUserFrequencyCheck)
                .Returns(disableUserFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_MaxFrequencyPerUser)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerUser));
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableIpAddressFrequencyCheck)
                .Returns(disableIpAddressFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_MaxFrequencyPerIpAddress)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerIpAddress));
            container.Rebind<IAntiAbuseServiceConfig>().ToConstant(mockAntiAbuseServiceConfig.Object);
        }

        private static void SetupContainerForCanCreateTenancyDetailsSubmission(IKernel container,
            bool disableUserFrequencyCheck, string maxFrequencyPerUser,
            bool disableIpAddressFrequencyCheck, string maxFrequencyPerIpAddress)

        {
            var parseHelper = new ParseHelper();
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_DisableUserFrequencyCheck)
                .Returns(disableUserFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_MaxFrequencyPerUser)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerUser));
            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck)
                .Returns(disableIpAddressFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerIpAddress));
            container.Rebind<IAntiAbuseServiceConfig>().ToConstant(mockAntiAbuseServiceConfig.Object);
        }

        private static void SetupContainerWithMockAdminAlertService(IKernel container, Action<string> callback)
        {
            var mockAdminAlertService = new Mock<IAdminAlertService>();

            mockAdminAlertService.Setup(x => x.SendAlert(It.IsAny<string>())).Callback<string>(callback);
            container.Rebind<IAdminAlertService>().ToConstant(mockAdminAlertService.Object);
        }
    }
}
