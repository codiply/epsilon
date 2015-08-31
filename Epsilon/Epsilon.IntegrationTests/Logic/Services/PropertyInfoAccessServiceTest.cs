using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.UserResidenceService;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class PropertyInfoAccessServiceTest : BaseIntegrationTestWithRollback
    {
        private TimeSpan _smallDelay = TimeSpan.FromMilliseconds(20);

        #region GetUserExploredPropertiesSummary

        [Test]
        public async Task GetUserExploredPropertiesSummary_ForUserWithoutPropertyInfoAccesses()
        {
            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);

            var random = new RandomWrapper(2015);

            // I create a property info access for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);
            var otherUserPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserPropertyInfoAccess, "The property info access created for the other user is null.");

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.IsFalse(response1.exploredProperties.Any(), "Field exploredProperties on response1 should be empty.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsFalse(response2.exploredProperties.Any(), "Field exploredProperties on response2 should be empty.");
        }

        [Test]
        public async Task GetUserExploredPropertiesSummary_WithPropertyInfoAccessesEqualToTheLimit_ItemsLimitIsNotRelevant()
        {
            var itemsLimit = 3;
            var propertyInfoAccessesToCreate = itemsLimit;
            var expiryPeriodInDays = 1.0;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccesses = new List<PropertyInfoAccess>();

            for (var i = 0; i <propertyInfoAccessesToCreate; i++)
            {
                var propertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                propertyInfoAccesses.Add(propertyInfoAccess);
                await Task.Delay(_smallDelay);
            }
            var propertyInfoAccessesByCreationDescending = propertyInfoAccesses.OrderByDescending(x => x.CreatedOn).ToList();

            // I create a property info access for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserPropertyInfoAccess, "The property info access created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserExploredPropertiesSummary(containerUnderTest, expiryPeriodInDays, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(propertyInfoAccessesToCreate, response1.exploredProperties.Count,
                "Response1 should contain all property info accesses.");
            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response1.exploredProperties[i].accessUniqueId,
                    string.Format("Response1: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info acccess of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.exploredProperties.Any(), "Field exploredProperties on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.exploredProperties.Count,
                "Response2 should contain a number of explored properties equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response2.exploredProperties[i].accessUniqueId,
                    string.Format("Response2: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info access of the other user.");
        }

        [Test]
        public async Task GetUserExploredPropertiesSummary_WithMorePropertyInfoAccessesThanTheLimit_ItemsLimitIsRespected()
        {
            var itemsLimit = 2;
            var propertyInfoAccessesToCreate = 3;
            var expiryPeriodInDays = 1.0;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccesses = new List<PropertyInfoAccess>();

            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                var propertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                propertyInfoAccesses.Add(propertyInfoAccess);
                await Task.Delay(_smallDelay);
            }
            var propertyInfoAccesseByCreationDescending = propertyInfoAccesses.OrderByDescending(x => x.CreatedOn).ToList();

            // I create a property info access for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserPropertyInfoAccess, "The property info access created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserExploredPropertiesSummary(containerUnderTest, expiryPeriodInDays, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(propertyInfoAccessesToCreate, response1.exploredProperties.Count,
                "Response1 should contain all explored properties.");
            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                Assert.AreEqual(propertyInfoAccesseByCreationDescending[i].UniqueId, response1.exploredProperties[i].accessUniqueId,
                    string.Format("Response1: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info access of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsTrue(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.exploredProperties.Any(), "Field exploredProperties on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.exploredProperties.Count,
                "Response2 should contain a number of explored properties equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(propertyInfoAccesseByCreationDescending[i].UniqueId, response2.exploredProperties[i].accessUniqueId,
                    string.Format("Response2: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info access of the other user.");
        }

        [Test]
        public async Task GetUserExploredPropertiesSummary_SingleExploredProperty_ExpiryTest()
        {
            var itemsLimit = 10;
            var expiryPeriod = TimeSpan.FromSeconds(0.3);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserExploredPropertiesSummary(containerUnderTest, expiryPeriodInDays, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var response = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.AreEqual(1, response.exploredProperties.Count,
                "The response should contain a single explored property.");

            var exploredProperty = response.exploredProperties.Single();

            Assert.AreEqual(propertyInfoAccess.UniqueId, exploredProperty.accessUniqueId,
                "Field accessUniqueId is not the expected.");
            Assert.IsTrue(exploredProperty.canViewInfo, "Field canViewInfo doesn't have the expected value.");

            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .Include(x => x.Address)
                .Include(x => x.Address.Country)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(propertyInfoAccess.UniqueId));
            Assert.AreEqual(retrievedPropertyInfoAccess.CreatedOn + expiryPeriod, exploredProperty.expiresOn,
                "Field expiresOn is not the expected.");
            Assert.AreEqual(retrievedPropertyInfoAccess.Address.FullAddress(), exploredProperty.displayAddress,
                "Field displayAddress is not the expected.");

            await Task.Delay(expiryPeriod);

            var responseAfterExpiry = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsEmpty(responseAfterExpiry.exploredProperties, "The explored property should not appear on the summary after the expiry.");
        }

        #endregion

        #region GetUserExploredPropertiesSummaryWithCaching

        [Test]
        public async Task GetUserExploredPropertiesSummaryWithCaching_WithPropertyInfoAccessesEqualToTheLimit_CachesTheSummary()
        {
            var itemsLimit = 3;
            var propertyInfoAccessesToCreate = itemsLimit;
            var expiryPeriodInDays = 1.0;
            var cachingPeriod = TimeSpan.FromSeconds(0.2);

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccesses = new List<PropertyInfoAccess>();

            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                var propertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                propertyInfoAccesses.Add(propertyInfoAccess);
                await Task.Delay(_smallDelay);
            }
            var propertyInfoAccessesByCreationDescending = propertyInfoAccesses.OrderByDescending(x => x.CreatedOn).ToList();

            // I create a property info access for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserPropertyInfoAccess, "The property info access created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserExploredPropertiesSummaryWithCaching(containerUnderTest, expiryPeriodInDays, itemsLimit, cachingPeriod);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummaryWithCaching(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(propertyInfoAccessesToCreate, response1.exploredProperties.Count,
                "Response1 should contain all property info accesses.");
            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response1.exploredProperties[i].accessUniqueId,
                    string.Format("Response1: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info acccess of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummaryWithCaching(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.exploredProperties.Any(), "Field exploredProperties on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.exploredProperties.Count,
                "Response2 should contain a number of explored properties equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response2.exploredProperties[i].accessUniqueId,
                    string.Format("Response2: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info access of the other user.");

            KillDatabase(containerUnderTest);
            var serviceWithoutDatabase = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response3 = await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, false);

            Assert.IsNotNull(response3, "Response3 is null.");
            Assert.IsFalse(response3.moreItemsExist, "Field moreItemsExist on response3 is not the expected.");
            Assert.AreEqual(propertyInfoAccessesToCreate, response3.exploredProperties.Count,
                "Response3 should contain all property info accesses.");
            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response3.exploredProperties[i].accessUniqueId,
                    string.Format("Response3: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response3.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response3 should not contain the property info acccess of the other user.");

            // Summary with limit
            var response4 = await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, true);

            Assert.IsNotNull(response4, "Response4 is null.");
            Assert.IsFalse(response4.moreItemsExist, "Field moreItemsExist on response4 is not the expected.");
            Assert.IsTrue(response4.exploredProperties.Any(), "Field exploredProperties on response4 should not be empty.");

            Assert.AreEqual(itemsLimit, response4.exploredProperties.Count,
                "Response4 should contain a number of explored properties equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response4.exploredProperties[i].accessUniqueId,
                    string.Format("Response4: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response4.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response4 should not contain the property info access of the other user.");

            await Task.Delay(cachingPeriod);

            Assert.Throws<ArgumentNullException>(async () => await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, false),
                "After the caching period is over, it should throw an exception because I have killed the  database. (limit items: false)");
            Assert.Throws<ArgumentNullException>(async () => await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, true),
                "After the caching period is over, it should throw an exception because I have killed the  database. (limit items: true)");

        }

        #endregion

        #region Private helper functions

        private static void SetupConfigForGetUserExploredPropertiesSummary(IKernel container, double expiryPeriodInDays, int itemsLimit)
        {
            var mockConfig = new Mock<IPropertyInfoAccessServiceConfig>();

            mockConfig.Setup(x => x.MyExploredPropertiesSummary_ItemsLimit).Returns(itemsLimit);
            mockConfig.Setup(x => x.ExpiryPeriodInDays).Returns(expiryPeriodInDays);

            container.Rebind<IPropertyInfoAccessServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static void SetupConfigForGetUserExploredPropertiesSummaryWithCaching(
            IKernel container, double expiryPeriodInDays, int itemsLimit, TimeSpan cachingPeriod)
        {
            var mockConfig = new Mock<IPropertyInfoAccessServiceConfig>();

            mockConfig.Setup(x => x.MyExploredPropertiesSummary_ItemsLimit).Returns(itemsLimit);
            mockConfig.Setup(x => x.ExpiryPeriodInDays).Returns(expiryPeriodInDays);
            mockConfig.Setup(x => x.MyExploredPropertiesSummary_CachingPeriod).Returns(cachingPeriod);

            container.Rebind<IPropertyInfoAccessServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static async Task<PropertyInfoAccess> CreatePropertyInfoAccessAndSave(
            IRandomWrapper random, IKernel container,
            string userId, string userIpAddress,
            string userIdForSubmission, string userForSubmissionIpAddress, int numberOfSubmission = 1, 
            CountryId countryId = CountryId.GB, CurrencyId currencyId = CurrencyId.GBP)
        {
            var clock = container.Get<IClock>();

            var address = await AddressHelper.CreateRandomAddressAndSave(
                random, container, userIdForSubmission, userForSubmissionIpAddress, countryId);

            var dbContext = container.Get<IEpsilonContext>();

            for (var i = 0; i < numberOfSubmission; i++)
            {
                var tenancyDetailsSubmission = new TenancyDetailsSubmission
                {
                    UniqueId = Guid.NewGuid(),
                    AddressId = address.Id,
                    UserId = userIdForSubmission,
                    CreatedByIpAddress = userForSubmissionIpAddress,
                    RentPerMonth = random.Next(100, 1000),
                    CurrencyId = EnumsHelper.CurrencyId.ToString(currencyId),
                    SubmittedOn = clock.OffsetNow
                };
                dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            }

            var propertyInfoAccess = new PropertyInfoAccess()
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                CreatedOn = clock.OffsetNow,
                UserId = userId,
                CreatedByIpAddress = userIpAddress
            };

            dbContext.PropertyInfoAccesses.Add(propertyInfoAccess);
            await dbContext.SaveChangesAsync();

            return propertyInfoAccess;
        }

        #endregion
    }
}
