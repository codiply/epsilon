using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
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

        #region CanRegister

        [Test]
        public async Task CanRegister_WithAllChecksDisabled_ReturnsIsRejectedFalse()
        {
            var usersToCreate = 10;
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

            for (int i = 0; i < usersToCreate; i++)
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

        #endregion

        #region CanAddAddress

        [Test]
        public async Task CanAddAddress_WithAllChecksDisabled_ReturnsIsRejectedFalse()
        {
            var addressesToCreate = 10;
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var container = CreateContainer();
            SetupContainerForCanAddAddress(container,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var service = container.Get<IAntiAbuseService>();

            var user = await CreateUser(container, "test@test.com", ipAddress);

            for (int i = 0; i < addressesToCreate; i++)
            {
                var address = await CreateAddress(container, user.Id, ipAddress);
            }

            var response = await service.CanAddAddress(user.Id, ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
        }

        [Test]
        public async Task CanAddAddress_CheckIpAddressFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddress(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            // I add the first address.
            var address1 = await CreateAddress(helperContainer, user.Id, ipAddress);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second user
            var address2 = await CreateAddress(helperContainer, user.Id, ipAddress);

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_IpAddressFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckIpAddressFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = String.Format("1/{0}S", periodInSeconds);

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddress(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var address = await CreateAddress(helperContainer, user.Id, ipAddress);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_IpAddressFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanAddAddress_CheckUserFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableUserFrequencyCheck = false;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";
            var ipAddress4 = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddress(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);

            // I add the first address.
            var address1 = await CreateAddress(helperContainer, user.Id, ipAddress1);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress2);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second user
            var address2 = await CreateAddress(helperContainer, user.Id, ipAddress3);
            
            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress4);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_UserFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckUserFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableUserFrequencyCheck = false;
            var maxFrequencyPerUser = String.Format("1/{0}S", periodInSeconds);
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddress(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);
            var address = await CreateAddress(helperContainer, user.Id, ipAddress1);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress2);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_UserFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress3);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        #endregion

        #region CanCreateTenancyDetailsSubmission

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_WithAllChecksDisabled_ReturnsIsRejectedFalse()
        {
            var submissionsToCreate = 10;
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var container = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(container,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var service = container.Get<IAntiAbuseService>();

            var user = await CreateUser(container, "test@test.com", ipAddress);

            for (int i = 0; i < submissionsToCreate; i++)
            {
                var submission = await CreateTenancyDetailsSubmission(container, user.Id, ipAddress);
            }

            var response = await service.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
        }

        [Test]
        public async Task CreateTenancyDetailsSubmission_CheckIpAddressFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            // I add the first address.
            var submission1 = await CreateTenancyDetailsSubmission(helperContainer, user.Id, ipAddress);

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second user
            var submission2 = await CreateTenancyDetailsSubmission(helperContainer, user.Id, ipAddress);

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_IpAddressFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CreateTenancyDetailsSubmission_CheckIpAddressFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = String.Format("1/{0}S", periodInSeconds);

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var submission = await CreateTenancyDetailsSubmission(helperContainer, user.Id, ipAddress);

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_IpAddressFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CreateTenancyDetailsSubmission_CheckUserFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableUserFrequencyCheck = false;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";
            var ipAddress4 = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);

            // I add the first address.
            var submission1 = await CreateTenancyDetailsSubmission(helperContainer, user.Id, ipAddress1);

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress2);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second user
            var submission2 = await CreateTenancyDetailsSubmission(helperContainer, user.Id, ipAddress3);

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress4);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CreateTenancyDetailsSubmission_CheckUserFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableUserFrequencyCheck = false;
            var maxFrequencyPerUser = String.Format("1/{0}S", periodInSeconds);
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);
            var submission = await CreateTenancyDetailsSubmission(helperContainer, user.Id, ipAddress1);

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress2);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress3);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        #endregion

        #region Private setup methods

        private static async Task<Address> CreateAddress(IKernel container, string userId, string userIpAddress)
        {
            var dbContext = container.Get<IEpsilonContext>();
            var random = container.Get<IRandomWrapper>();
            var address = new Address
            {
                Line1 = RandomStringHelper.GetString(random, 10, RandomStringHelper.CharacterCase.Mixed),
                Locality = RandomStringHelper.GetString(random, 10, RandomStringHelper.CharacterCase.Mixed),
                Postcode = RandomStringHelper.GetString(random, 10, RandomStringHelper.CharacterCase.Mixed),
                CountryId = EnumsHelper.CountryId.ToString(CountryId.GB),
                CreatedById = userId,
                CreatedByIpAddress = userIpAddress
            };
            dbContext.Addresses.Add(address);
            await dbContext.SaveChangesAsync();
            return address;
        }

        private static async Task<TenancyDetailsSubmission> CreateTenancyDetailsSubmission(IKernel container, string userId, string userIpAddress)
        {
            var address = await CreateAddress(container, userId, userIpAddress);
            var dbContext = container.Get<IEpsilonContext>();

            var random = container.Get<IRandomWrapper>();
            var tenancyDetailsSubmission = new TenancyDetailsSubmission
            {
                AddressId = address.Id,
                UserId = userId,
                CreatedByIpAddress = userIpAddress
            };
            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();
            return tenancyDetailsSubmission;
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

        #endregion
    }
}
