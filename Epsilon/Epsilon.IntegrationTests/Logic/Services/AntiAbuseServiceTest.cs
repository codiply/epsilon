using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.IntegrationTests.TestHelpers;
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
using Epsilon.Logic.Wrappers;
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
        #region CanAddAddress

        [Test]
        public async Task CanAddAddress_WithAllChecksDisabled_ReturnsIsRejectedFalse()
        {
            var addressesToCreate = 5;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var container = CreateContainer();
            SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(container,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var service = container.Get<IAntiAbuseService>();

            var user = await CreateUser(container, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);
            for (int i = 0; i < addressesToCreate; i++)
            {
                var address = await AddressHelper.CreateRandomAddressAndSave(random, container, user.Id, ipAddress, CountryId.GB);
                Assert.IsNotNull(address, "The address created for i {0} is null.");
            }

            var response = await service.CanAddAddress(user.Id, ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
            Assert.IsNullOrEmpty(response.RejectionReason, "The rejection reason should have no value.");
        }

        [Test]
        public async Task CanAddAddress_CheckGlobalFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = "2/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            string adminAlertKey = string.Empty;
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            // I add the first address.
            var address1 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");
            Assert.AreEqual(string.Empty, adminAlertKey, "Admin alert should not be sent the first time.");
            Assert.IsNull(adminEventLogKey, "Admin event should not be logged the first time.");

            // I add the second address.
            var address2 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_GlobalFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.IsNotNull(adminAlertKey, "The AdminAlertService was not called.");
            Assert.AreEqual(AdminAlertKey.AddAddressGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the second time.");
            Assert.IsNotNull(adminEventLogKey, "The AdminEventLogService was not called.");
            Assert.AreEqual(AdminEventLogKey.AddAddressGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log key was not used the second time.");
        }

        [Test]
        public async Task AddAddress_CheckGlobalFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = string.Format("1/{0}S", periodInSeconds);
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            string adminAlertKey = string.Empty;
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress1, CountryId.GB);
            Assert.IsNotNull(address, "Address created is null.");

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress2);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_GlobalFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.AreEqual(AdminAlertKey.AddAddressGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the first time.");
            Assert.AreEqual(AdminEventLogKey.AddAddressGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log was not created the first time.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress3);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanAddAddress_CheckIpAddressFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            // I add the first address.
            var address1 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second address.
            var address2 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_IpAddressFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckIpAddressFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = string.Format("1/{0}S", periodInSeconds);

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);
            Assert.IsNotNull(address, "Address created is null.");

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
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableUserFrequencyCheck = false;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";
            var ipAddress4 = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);

            // I add the first address.
            var address1 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress1, CountryId.GB);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress2);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second address
            var address2 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress3, CountryId.GB);
            
            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress4);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_UserFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckUserFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableUserFrequencyCheck = false;
            var maxFrequencyPerUser = string.Format("1/{0}S", periodInSeconds);
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);
            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress1, CountryId.GB);
            Assert.IsNotNull(address, "Address created is null.");

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress2);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_UserFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress3);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanAddAddress_GeocodeFailureCheckIpAddressFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGeocodeFailureUserFrequencyCheck = true;
            var maxGeocodeFailureFrequencyPerUser = "2/H";
            var disableGeocodeFailureIpAddressFrequencyCheck = false;
            var maxGeocodeFailureFrequencyPerIpAddress = "2/H";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithOnlyGeocodeFailureCheck(containerUnderTest,
                disableGeocodeFailureUserFrequencyCheck, maxGeocodeFailureFrequencyPerUser,
                disableGeocodeFailureIpAddressFrequencyCheck, maxGeocodeFailureFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            
            // I add the first failure.
            var geocodeFailure1 = await CreateGeocodeFailureAndSave(helperContainer, user.Id, ipAddress);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second failure.
            var geocodeFailure2 = await CreateGeocodeFailureAndSave(helperContainer, user.Id, ipAddress);

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_GeocodeFailureIpAddressFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_GeocodeFailureCheckIpAddressFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGeocodeFailureUserFrequencyCheck = true;
            var maxGeocodeFailureFrequencyPerUser = "2/H";
            var disableGeocodeFailureIpAddressFrequencyCheck = false;
            var maxGeocodeFailureFrequencyPerIpAddress = string.Format("1/{0}S", periodInSeconds);

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithOnlyGeocodeFailureCheck(containerUnderTest,
                disableGeocodeFailureUserFrequencyCheck, maxGeocodeFailureFrequencyPerUser,
                disableGeocodeFailureIpAddressFrequencyCheck, maxGeocodeFailureFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var geocodeFailure = await CreateGeocodeFailureAndSave(helperContainer, user.Id, ipAddress);
            Assert.IsNotNull(geocodeFailure, "GeocodeFailure created is null.");

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_GeocodeFailureIpAddressFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanAddAddress_GeocodeFailureCheckUserFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGeocodeFailureUserFrequencyCheck = false;
            var maxGeocodeFailureFrequencyPerUser = "2/H";
            var disableGeocodeFailureIpAddressFrequencyCheck = true;
            var maxGeocodeFailureFrequencyPerIpAddress = "2/H";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithOnlyGeocodeFailureCheck(containerUnderTest,
                disableGeocodeFailureUserFrequencyCheck, maxGeocodeFailureFrequencyPerUser,
                disableGeocodeFailureIpAddressFrequencyCheck, maxGeocodeFailureFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);

            // I add the first failure.
            var geocodeFailure1 = await CreateGeocodeFailureAndSave(helperContainer, user.Id, ipAddress1);

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress2);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second failure
            var geocodeFailure2 = await CreateGeocodeFailureAndSave(helperContainer, user.Id, ipAddress1);

            var secondResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress3);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_GeocodeFailureUserFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_GeocodeFailureCheckUserFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGeocodeFailureUserFrequencyCheck = false;
            var maxGeocodeFailureFrequencyPerUser = string.Format("1/{0}S", periodInSeconds);
            var disableGeocodeFailureIpAddressFrequencyCheck = true;
            var maxGeocodeFailureFrequencyPerIpAddress = "2/H";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanAddAddressWithOnlyGeocodeFailureCheck(containerUnderTest,
                disableGeocodeFailureUserFrequencyCheck, maxGeocodeFailureFrequencyPerUser,
                disableGeocodeFailureIpAddressFrequencyCheck, maxGeocodeFailureFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);
            var geocodeFailure = await CreateGeocodeFailureAndSave(helperContainer, user.Id, ipAddress1);
            Assert.IsNotNull(geocodeFailure, "GeocodeFailure created is null.");

            var firstResponse = await serviceUnderTest.CanAddAddress(user.Id, ipAddress2);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.AddAddress_GeocodeFailureUserFrequencyCheck_RejectionMessage,
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
            var submissionsToCreate = 5;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var container = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(container,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var service = container.Get<IAntiAbuseService>();

            var user = await CreateUser(container, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);
            for (int i = 0; i < submissionsToCreate; i++)
            {
                var submission = await CreateTenancyDetailsSubmissionAndSave(random, container, user.Id, ipAddress);
                Assert.IsNotNull(submission, "The submission created for i {0} is null.");
            }

            var response = await service.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
            Assert.IsNullOrEmpty(response.RejectionReason, "The rejection reason should have no value.");
        }

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_CheckGlobalFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = "2/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            string adminAlertKey = string.Empty;
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);


            // I add the first submission.
            var submission1 = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress);

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");
            Assert.AreEqual(string.Empty, adminAlertKey, "Admin alert should not be sent the first time.");
            Assert.IsNull(adminEventLogKey, "Admin event should not be logged the first time.");

            // I add the second submission.
            var submission2 = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress);

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_GlobalFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.IsNotNull(adminAlertKey, "The AdminAlertService was not called.");
            Assert.AreEqual(AdminAlertKey.CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the second time.");
            Assert.IsNotNull(adminEventLogKey, "The AdminEventLogService was not called.");
            Assert.AreEqual(AdminEventLogKey.CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log key was not used the second time.");
        }

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_CheckGlobalFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = string.Format("1/{0}S", periodInSeconds);
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";


            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            string adminAlertKey = string.Empty;
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var submission = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress);
            Assert.IsNotNull(submission, "TenancyDetailsSubmission created is null.");

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_GlobalFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.AreEqual(AdminAlertKey.CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the first time.");
            Assert.AreEqual(AdminEventLogKey.CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log was not created the first time.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CreateTenancyDetailsSubmission_CheckIpAddressFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            // I add the first submission.
            var submission1 = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress);

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second submission.
            var submission2 = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress);

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_IpAddressFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CreateTenancyDetailsSubmission_CheckIpAddressFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableUserFrequencyCheck = true;
            var maxFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = string.Format("1/{0}S", periodInSeconds);

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var submission = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress);
            Assert.IsNotNull(submission, "TenancyDetailsSubmission created is null.");

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
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
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
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);

            // I add the first submission.
            var submission1 = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress1);
            Assert.IsNotNull(submission1, "Submission1 is null.");

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress2);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second submission.
            var submission2 = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress3);
            Assert.IsNotNull(submission1, "Submission2 is null.");

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress4);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CreateTenancyDetailsSubmission_CheckUserFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableUserFrequencyCheck = false;
            var maxFrequencyPerUser = string.Format("1/{0}S", periodInSeconds);
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress1 = "1.2.3.1";
            var ipAddress2 = "1.2.3.2";
            var ipAddress3 = "1.2.3.3";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanCreateTenancyDetailsSubmission(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableUserFrequencyCheck, maxFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();
            var random = new RandomWrapper(2015);

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress1);
            var submission = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress1);
            Assert.IsNotNull(submission, "TenancyDetailsSubmission created is null.");

            var firstResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress2);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanCreateTenancyDetailsSubmission(user.Id, ipAddress3);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        #endregion

        #region CanPickOutgoingVerification

        [Test]
        public async Task CanPickOutgoingVerification_WithAllChecksDisabled_ReturnsIsRejectedFalse()
        {
            var verificationsToCreate = 5;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableMaxOutstandingPerUserCheck = true;
            var maxOutstandingFrequencyPerUserForNewUser = "2/D";
            var maxOutstandingFrequencyPerUser = "4/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var container = CreateContainer();
            SetupContainerForCanPickOutgoingVerification(container,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var service = container.Get<IAntiAbuseService>();

            var user = await CreateUser(container, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);
            for (int i = 0; i < verificationsToCreate; i++)
            {
                var verification = await CreateTenantVerificationAndSave(random, container, user.Id, ipAddress, false);
                Assert.IsNotNull(verification, "The verification created for i {0} is null.");
            }

            var response = await service.CanPickOutgoingVerification(user.Id, ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
            Assert.IsNullOrEmpty(response.RejectionReason, "The rejection reason should have no value.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckGlobalFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = "2/D";
            var disableMaxOutstandingPerUserCheck = true;
            var maxOutstandingFrequencyPerUserForNewUser = "2/D";
            var maxOutstandingFrequencyPerUser = "4/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();

            string adminAlertKey = string.Empty;
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            // I add the first verification.
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");
            Assert.AreEqual(string.Empty, adminAlertKey, "Admin alert should not be sent the first time.");
            Assert.IsNull(adminEventLogKey, "Admin event should not be logged the first time.");

            // I add the second verification.
            var verification2 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_GlobalFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.IsNotNull(adminAlertKey, "The AdminAlertService was not called.");
            Assert.AreEqual(AdminAlertKey.PickOutgoingVerificationGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the second time.");
            Assert.IsNotNull(adminEventLogKey, "The AdminEventLogService was not called.");
            Assert.AreEqual(AdminEventLogKey.PickOutgoingVerificationGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log key was not used the second time.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckGlobalFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = string.Format("1/{0}S", periodInSeconds);
            var disableMaxOutstandingPerUserCheck = true;
            var maxOutstandingFrequencyPerUserForNewUser = "2/D";
            var maxOutstandingFrequencyPerUser = "4/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();

            string adminAlertKey = string.Empty;
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var verification = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);
            Assert.IsNotNull(verification, "TenantVerifiation created is null.");

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_GlobalFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.AreEqual(AdminAlertKey.PickOutgoingVerificationGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the first time.");
            Assert.AreEqual(AdminEventLogKey.PickOutgoingVerificationGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log was not created the first time.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckIpAddressFrequency_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableMaxOutstandingPerUserCheck = true;
            var maxOutstandingFrequencyPerUserForNewUser = "2/D";
            var maxOutstandingFrequencyPerUser = "4/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            // I add the first verification.
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second verification.
            var verification2 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_IpAddressFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckIpAddressFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "10/D";
            var disableMaxOutstandingPerUserCheck = true;
            var maxOutstandingFrequencyPerUserForNewUser = "2/D";
            var maxOutstandingFrequencyPerUser = "4/D";
            var disableIpAddressFrequencyCheck = false;
            var maxFrequencyPerIpAddress = string.Format("1/{0}S", periodInSeconds);

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var verification = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);
            Assert.IsNotNull(verification, "TenantVerifiation created is null.");

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_IpAddressFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckMaxOutstandingFrequencyForUser_ForNewUser_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableMaxOutstandingPerUserCheck = false;
            var maxOutstandingFrequencyPerUserForNewUser = "2/D";
            var maxOutstandingFrequencyPerUser = "0/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            // I add the first verification.
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second verification.
            var verification2 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_MaxOutstandingFrequencyPerUserCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckMaxOutstandingFrequencyForUser_ForNewUser_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableMaxOutstandingPerUserCheck = false;
            var maxOutstandingFrequencyPerUserForNewUser = string.Format("1/{0}S", periodInSeconds);
            var maxOutstandingFrequencyPerUser = "0/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var verification = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);
            Assert.IsNotNull(verification, "TenantVerifiation created is null.");

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_MaxOutstandingFrequencyPerUserCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckMaxOutstandingFruquencyForUser_ForUserWithCompleteVerifications_TheFrequencyTimesIsUsedCorrectly()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableMaxOutstandingPerUserCheck = false;
            var maxOutstandingFrequencyPerUserForNewUser = "0/D";
            var maxOutstandingFrequencyPerUser = "2/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var completeVerification = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, true);

            // I add the first verification.
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");

            // I add the second verification.
            var verification2 = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_MaxOutstandingFrequencyPerUserCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
        }


        [Test]
        public async Task CanPickOutgoingVerification_CheckMaxOutstandingFrequencyForUser_ForUserWithCompleteVerifications_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "2/D";
            var disableMaxOutstandingPerUserCheck = false;
            var maxOutstandingFrequencyPerUserForNewUser = "0/D";
            var maxOutstandingFrequencyPerUser = string.Format("1/{0}S", periodInSeconds);
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "2/D";

            var ipAddress = "1.2.3.4";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanPickOutgoingVerification(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableMaxOutstandingPerUserCheck, maxOutstandingFrequencyPerUserForNewUser, maxOutstandingFrequencyPerUser,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var completeVerification = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, true);
            Assert.IsNotNull(completeVerification, "Complete TenantVerifiation created is null.");
            var incompleteVerification = await CreateTenantVerificationAndSave(random, helperContainer, user.Id, ipAddress, false);
            Assert.IsNotNull(completeVerification, "Incomplete TenantVerifiation created is null.");

            var firstResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.PickOutgoingVerification_MaxOutstandingFrequencyPerUserCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");

            await Task.Delay(TimeSpan.FromSeconds(periodInSeconds));

            var secondResponse = await serviceUnderTest.CanPickOutgoingVerification(user.Id, ipAddress);
            Assert.IsFalse(secondResponse.IsRejected, "The request should not be rejected the second time.");
        }

        #endregion

        #region CanRegister

        [Test]
        public async Task CanRegister_WithAllChecksDisabled_ReturnsIsRejectedFalse()
        {
            var usersToCreate = 5;
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
                var user = await CreateUser(container, string.Format("test{0}@test.com", i), ipAddress);
                Assert.IsNotNull(user, "The user created for i {0} is null.");
            }

            var response = await service.CanRegister(ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
            Assert.IsNullOrEmpty(response.RejectionReason, "The rejection reason should have no value.");
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
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            // I create the first user.
            var user1 = await CreateUser(helperContainer, "test1@test.com", "1.2.3.1");

            var firstResponse = await serviceUnderTest.CanRegister("1.2.3.2");
            Assert.IsFalse(firstResponse.IsRejected, "The first check should pass.");
            Assert.AreEqual(string.Empty, adminAlertKey, "Admin alert should not be sent the first time.");
            Assert.IsNull(adminEventLogKey, "Admin event should not be logged the first time.");

            // I create the second user
            var user2 = await CreateUser(helperContainer, "test2@test.com", "1.2.3.3");

            var secondResponse = await serviceUnderTest.CanRegister("1.2.3.4");
            Assert.IsTrue(secondResponse.IsRejected, "The second check should fail.");
            Assert.AreEqual(AntiAbuseResources.Register_GlobalFrequencyCheck_RejectionMessage,
                secondResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.IsNotNull(adminAlertKey, "The AdminAlertService was not called.");
            Assert.AreEqual(AdminAlertKey.RegistrationGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the second time.");
            Assert.IsNotNull(adminEventLogKey, "The AdminEventLogService was not called.");
            Assert.AreEqual(AdminEventLogKey.RegistrationGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log key was not used the second time.");
        }

        [Test]
        public async Task CanRegister_CheckGlobalFrequency_TheFrequencyPeriodIsUsedCorrectly()
        {
            var periodInSeconds = 0.2;
            var disableGlobalFrequencyCheck = false;
            var globalMaxFrequency = string.Format("1/{0}S", periodInSeconds);
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var containerUnderTest = CreateContainer();
            SetupContainerForCanRegister(containerUnderTest,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);

            string adminAlertKey = string.Empty;
            AdminEventLogKey? adminEventLogKey = null;
            SetupContainerWithMockAdminAlertService(containerUnderTest, x => adminAlertKey = x);
            SetupContainerWithMockAdminEventLogService(containerUnderTest, (x, info) => adminEventLogKey = x);

            var serviceUnderTest = containerUnderTest.Get<IAntiAbuseService>();

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", "1.2.3.4");

            var firstResponse = await serviceUnderTest.CanRegister("1.2.3.4");
            Assert.IsTrue(firstResponse.IsRejected, "The first check should fail.");
            Assert.AreEqual(AntiAbuseResources.Register_GlobalFrequencyCheck_RejectionMessage,
                firstResponse.RejectionReason, "The rejection reason is not the expected.");
            Assert.AreEqual(AdminAlertKey.RegistrationGlobalMaxFrequencyReached, adminAlertKey,
                "The right admin alert was not send the first time.");
            Assert.AreEqual(AdminEventLogKey.RegistrationGlobalMaxFrequencyReached, adminEventLogKey,
                "The right admin event log was not created the first time.");

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
            var maxFrequencyPerIpAddress = string.Format("1/{0}S", periodInSeconds);
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

        #region Private setup methods

        private static async Task<TenancyDetailsSubmission> CreateTenancyDetailsSubmissionAndSave(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress)
        {
            var address = await AddressHelper.CreateRandomAddressAndSave(random, container, userId, userIpAddress, CountryId.GB);
            var dbContext = container.Get<IEpsilonContext>();
            
            var tenancyDetailsSubmission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                UserId = userId,
                CreatedByIpAddress = userIpAddress
            };
            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();
            return tenancyDetailsSubmission;
        }

        private static async Task<TenantVerification> CreateTenantVerificationAndSave(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress, bool isComplete)
        {
            var clock = container.Get<IClock>();
            var dbContext = container.Get<IEpsilonContext>();

            var tenancyDetailsSubmission = await CreateTenancyDetailsSubmissionAndSave(random, container, userId, userIpAddress);

            var tenantVerification = new TenantVerification
            {
                UniqueId = Guid.NewGuid(),
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = "secret",
                TenancyDetailsSubmissionId = tenancyDetailsSubmission.Id
            };
            if (isComplete)
                tenantVerification.VerifiedOn = clock.OffsetNow;
            dbContext.TenantVerifications.Add(tenantVerification);
            await dbContext.SaveChangesAsync();
            return tenantVerification;
        }

        private static async Task<GeocodeFailure> CreateGeocodeFailureAndSave(
            IKernel container, string userId, string userIpAddress)
        {
            var dbContext = container.Get<IEpsilonContext>();

            var geocodeFailure = new GeocodeFailure
            {
                Address = "Geocode-Failure-Address",
                CountryId = "GB",
                CreatedById = userId,
                CreatedByIpAddress = userIpAddress
            };
            dbContext.GeocodeFailures.Add(geocodeFailure);
            await dbContext.SaveChangesAsync();
            return geocodeFailure;
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

        private static void SetupContainerForCanAddAddressWithoutGeocodeFailureCheck(IKernel container,
            bool disableGlobalFrequencyCheck, string globalMaxFrequency,
            bool disableUserFrequencyCheck, string maxFrequencyPerUser,
            bool disableIpAddressFrequencyCheck, string maxFrequencyPerIpAddress)
        {
            var parseHelper = new ParseHelper();
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGlobalFrequencyCheck)
                .Returns(disableGlobalFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_GlobalMaxFrequency)
                .Returns(parseHelper.ParseFrequency(globalMaxFrequency));
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableUserFrequencyCheck)
                .Returns(disableUserFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_MaxFrequencyPerUser)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerUser));
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableIpAddressFrequencyCheck)
                .Returns(disableIpAddressFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_MaxFrequencyPerIpAddress)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerIpAddress));

            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck)
                .Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGeocodeFailureUserFrequencyCheck)
                .Returns(true);
            container.Rebind<IAntiAbuseServiceConfig>().ToConstant(mockAntiAbuseServiceConfig.Object);
        }

        private static void SetupContainerForCanAddAddressWithOnlyGeocodeFailureCheck(IKernel container,
            bool disableGeocodeFailureUserFrequencyCheck, string maxGeocodeFailureFrequencyPerUser,
            bool disableGeocodeFailureIpAddressFrequencyCheck, string maxGeocodeFailureFrequencyPerIpAddress)
        {
            var parseHelper = new ParseHelper();
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGlobalFrequencyCheck)
                .Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGeocodeFailureUserFrequencyCheck)
                .Returns(disableGeocodeFailureUserFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_MaxGeocodeFailureFrequencyPerUser)
                .Returns(parseHelper.ParseFrequency(maxGeocodeFailureFrequencyPerUser));
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck)
                .Returns(disableGeocodeFailureIpAddressFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_MaxGeocodeFailureFrequencyPerIpAddress)
                .Returns(parseHelper.ParseFrequency(maxGeocodeFailureFrequencyPerIpAddress));

            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableUserFrequencyCheck)
                .Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableIpAddressFrequencyCheck)
                .Returns(true);
            container.Rebind<IAntiAbuseServiceConfig>().ToConstant(mockAntiAbuseServiceConfig.Object);
        }

        private static void SetupContainerForCanPickOutgoingVerification(IKernel container,
            bool disableGlobalFrequencyCheck, string globalMaxFrequency,
            bool disableMaxOutstandingPerUserCheck, string maxOutstandingFrequencyPerUserForNewUser, string maxOutstandingFrequencyPerUser,
            bool disableIpAddressFrequencyCheck, string maxFrequencyPerIpAddress)
        {
            var parseHelper = new ParseHelper();
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_DisableGlobalFrequencyCheck)
                .Returns(disableGlobalFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_GlobalMaxFrequency)
                .Returns(parseHelper.ParseFrequency(globalMaxFrequency));

            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_DisableMaxOutstandingFrequencyPerUserCheck)
                .Returns(disableMaxOutstandingPerUserCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser)
                .Returns(parseHelper.ParseFrequency(maxOutstandingFrequencyPerUserForNewUser));
            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_MaxOutstandingFrequencyPerUser)
                .Returns(parseHelper.ParseFrequency(maxOutstandingFrequencyPerUser));

            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_DisableIpAddressFrequencyCheck)
                .Returns(disableIpAddressFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_MaxFrequencyPerIpAddress)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerIpAddress));

            container.Rebind<IAntiAbuseServiceConfig>().ToConstant(mockAntiAbuseServiceConfig.Object);
        }

        private static void SetupContainerForCanCreateTenancyDetailsSubmission(IKernel container,
            bool disableGlobalFrequencyCheck, string globalMaxFrequency,
            bool disableUserFrequencyCheck, string maxFrequencyPerUser,
            bool disableIpAddressFrequencyCheck, string maxFrequencyPerIpAddress)
        {
            var parseHelper = new ParseHelper();
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck)
                .Returns(disableGlobalFrequencyCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_GlobalMaxFrequency)
                .Returns(parseHelper.ParseFrequency(globalMaxFrequency));
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

        private static void SetupContainerWithMockAdminEventLogService(IKernel container, Action<AdminEventLogKey, Dictionary<string, object>> callback)
        {
            var mockAdminEventLogService = new Mock<IAdminEventLogService>();

            mockAdminEventLogService.Setup(x => x.Log(It.IsAny<AdminEventLogKey>(), It.IsAny<Dictionary<string, object>>()))
                .Callback(callback)
                .Returns(Task.FromResult(true));
            container.Rebind<IAdminEventLogService>().ToConstant(mockAdminEventLogService.Object);
        }

        #endregion
    }
}
