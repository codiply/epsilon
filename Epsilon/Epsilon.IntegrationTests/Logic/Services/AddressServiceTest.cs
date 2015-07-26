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
using NUnit.Framework;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Forms.Submission;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AddressServiceTest : BaseIntegrationTestWithRollback
    {
        #region Search

        [Test]
        public async Task Search_ExactPostcode_AndResultsCountEqualToLimit()
        {
            int searchAddressResultsLimit = 10;
            int numberOfAddressesToCreate = searchAddressResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, numberOfAddressesToCreate, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.Search(request1);

            Assert.AreEqual(addresses.Count, response1.results.Count,
                "A request with exact postcode (case-insensitive) should match all the addresses.");

            var request2 = new AddressSearchRequest { countryId = countryId, postcode = "POSTCO", terms = "" };
            var response2 = await serviceUnderTest.Search(request2);

            Assert.IsEmpty(response2.results, "A request with part of the postcode should return no results.");
            Assert.IsFalse(response2.isResultsLimitExceeded, 
                "The results limit should not be flagged as exceeded when the number of results is equal to the limit.");
            Assert.AreEqual(searchAddressResultsLimit, response2.resultsLimit, 
                "The results limit reported on the response was not the expected."); 
        }

        [Test]
        public async Task Search_SearchesTermsInAllFields()
        {
            int searchAddressResultsLimit = 3;
            int numberOfAddressesToCreate = searchAddressResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, numberOfAddressesToCreate, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.Search(request1);

            var addressToSearch = addresses.First();

            // I use each field as a term and separate them in different ways in the search terms string.
            var searchTermsField = string.Format(" {0},{1}, {2} ,{3} {4}   {5}, ",
                addressToSearch.Line1, addressToSearch.Line2, addressToSearch.Line3, addressToSearch.Line4,
                addressToSearch.Locality, addressToSearch.Region);

            var request = new AddressSearchRequest { countryId = countryId, postcode = postcode, terms = searchTermsField };
            var response = await serviceUnderTest.Search(request);

            Assert.AreEqual(1, response.results.Count, "The response should contain a single result.");
            Assert.IsFalse(response.isResultsLimitExceeded, "The results limit should not be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.resultsLimit, 
                "The results limit on the response was not the expected.");

            var returnedAddress = response.results.Single();

            Assert.AreEqual(addressToSearch.UniqueId, returnedAddress.addressUniqueId, "The id of the address returned is not the expected.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(addressToSearch.Line1), "Line1 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(addressToSearch.Line2), "Line2 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(addressToSearch.Line3), "Line3 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(addressToSearch.Line4), "Line4 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(addressToSearch.Locality), "Locality was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(addressToSearch.Region), "Region was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(addressToSearch.Postcode), "Postcode was not found in the full address.");
        }

        [Test]
        public async Task Search_DoesNotUseSearchTermsToSearchCountryIdOrPostcode()
        {
            int searchAddressResultsLimit = 3;
            int numberOfAddressesToCreate = searchAddressResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, numberOfAddressesToCreate, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.Search(request1);

            var addressToSearch = addresses.First();

            var searchTermsField = string.Format("{0} {1}", postcode, countryId);

            var request = new AddressSearchRequest { countryId = countryId, postcode = postcode, terms = searchTermsField };
            var response = await serviceUnderTest.Search(request);

            Assert.IsEmpty(response.results, "The response should contain no results.");
            Assert.IsFalse(response.isResultsLimitExceeded, "The results limit should not be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.resultsLimit,
                "The results limit on the response was not the expected.");
        }

        [Test]
        public async Task Search_ReturnsNoMoreAddressesThanTheLimit()
        {
            int searchAddressResultsLimit = 3;
            int numberOfAddressesToCreate = searchAddressResultsLimit + 1;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, numberOfAddressesToCreate, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response = await serviceUnderTest.Search(request);

            Assert.AreEqual(searchAddressResultsLimit, response.results.Count,
                "The number of addresses returned should equal the results limit.");
            Assert.IsTrue(response.isResultsLimitExceeded,
                "The results limit should be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.resultsLimit,
                "The results limit reported on the response was not the expected.");
        }

        #endregion

        #region AddAddress

        [Test]
        public async Task AddAddress_FollowedBy_GetAddressViaUniqueId_AreConsistent()
        {
            
            var ipAddress = "1.2.3.4";
            var countryId = "GB";

            var addressGeometry = new AddressGeometry
            {
                Latitude = 1.0,
                Longitude = 2.0,
                ViewportNortheastLatitude = 3.0,
                ViewportNortheastLongitude = 4.0,
                ViewportSouthwestLatitude = 5.0,
                ViewportSouthwestLongitude = 6.0
            };

            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;
            AddressForm addressFormUsedInVerification = null;
            var userIdUsedInVerification = string.Empty;
            var ipAddressUsedInVerification = string.Empty;

            var containerForAdd = CreateContainer();
            SetupAntiAbuseServiceResponse(containerForAdd, (userId, ipAddr) => 
                {
                    userIdUsedInAntiAbuse = userId;
                    ipAddressUsedInAntiAbuse = ipAddr;
                }, new AntiAbuseServiceResponse
                {
                    IsRejected = false
                });
            SetupAddressVerficationServiceResponse(containerForAdd, (userId, userIpAddress, form) => {
                    userIdUsedInVerification = userId;
                    ipAddressUsedInVerification = userIpAddress;
                    addressFormUsedInVerification = form;
                },
                new AddressVerificationResponse
                {
                    IsRejected = false,
                    AddressGeometry = addressGeometry
                });
            var serviceForAdd = containerForAdd.Get<IAddressService>();

            var addressForm = CreateRandomAddresForm(countryId);
            var postcodeGeometry = await CreatePostcodeGeometry(helperContainer, addressForm.CountryId, addressForm.Postcode);

            var timeBefore = DateTimeOffset.Now;
            var outcome = await serviceForAdd.AddAddress(user.Id, ipAddress, addressForm);

            Assert.IsFalse(outcome.IsRejected, "IsRejected on the outcome should be false.");
            Assert.AreEqual(addressForm.UniqueId, outcome.AddressUniqueId, "The AddressId on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The rejection reason should not be populated.");
            var timeAfter = DateTimeOffset.Now;

            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, "UserId used in AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, "IpAddress used in AntiAbuseService is not the expected");
            Assert.IsNotNull(addressFormUsedInVerification, "Address form used in VerificationService is null.");
            Assert.AreEqual(addressForm.UniqueId, addressFormUsedInVerification.UniqueId, 
                "AddressId on the form submitted for verification is not the expected.");
            Assert.AreEqual(user.Id, userIdUsedInVerification, "UserId used in Verification is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInVerification, "IpAddress used in Verification is not the expected.");

            var containerForGet = CreateContainer();
            var serviceForGet = containerForGet.Get<IAddressService>();

            var retrievedAddress = await serviceForGet.GetAddressWithGeometries(addressForm.UniqueId);

            Assert.IsNotNull(retrievedAddress, "Address could not be retrieved.");
            Assert.AreEqual(addressForm.Line1, retrievedAddress.Line1, "Field Line1 on the retrieved address is not the expected.");
            Assert.AreEqual(addressForm.Line2, retrievedAddress.Line2, "Field Line2 on the retrieved address is not the expected.");
            Assert.AreEqual(addressForm.Line3, retrievedAddress.Line3, "Field Line3 on the retrieved address is not the expected.");
            Assert.AreEqual(addressForm.Line4, retrievedAddress.Line4, "Field Line4 on the retrieved address is not the expected.");
            Assert.AreEqual(addressForm.Locality, retrievedAddress.Locality, "Field Locality on the retrieved address is not the expected.");
            Assert.AreEqual(addressForm.Region, retrievedAddress.Region, "Field Region on the retrieved address is not the expected.");
            Assert.AreEqual(addressForm.CountryId, retrievedAddress.CountryId, "Field CountryId on the retrieved address is not the expected.");
            Assert.AreEqual(user.Id, retrievedAddress.CreatedById, "Field CreatedById on the retrieved address is not the expected.");
            Assert.AreEqual(ipAddress, retrievedAddress.CreatedByIpAddress, "Field CreatedByIpAddress on the retrieved address is not the expected.");
            Assert.IsTrue(timeBefore <= retrievedAddress.CreatedOn && retrievedAddress.CreatedOn <= timeAfter,
                "Field CreatedOn on the retrieved address is not within the expected range.");

            Assert.IsNotNull(retrievedAddress.Geometry, "Geometry field on retrieved address is null.");
            Assert.AreEqual(addressGeometry.Latitude, retrievedAddress.Geometry.Latitude,
                "Field Latitude on Geometry of the retrieved address is not the expected.");
            Assert.AreEqual(addressGeometry.Longitude, retrievedAddress.Geometry.Longitude,
                "Field Longitude on Geometry of the retrieved address is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportNortheastLatitude, retrievedAddress.Geometry.ViewportNortheastLatitude,
                "Field ViewportNortheastLatitude on Geometry of the retrieved address is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportNortheastLongitude, retrievedAddress.Geometry.ViewportNortheastLongitude,
                "Field ViewportNortheastLongitude on Geometry of the retrieved address is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportSouthwestLatitude, retrievedAddress.Geometry.ViewportSouthwestLatitude,
                "Field ViewportSouthwestLatitude on Geometry of the retrieved address is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportSouthwestLongitude, retrievedAddress.Geometry.ViewportSouthwestLongitude,
                "Field ViewportSouthwestLatitude on Geometry of the retrieved address is not the expected.");
            Assert.IsTrue(timeBefore <= retrievedAddress.Geometry.GeocodedOn && retrievedAddress.Geometry.GeocodedOn <= timeAfter,
                "Field GeocodedOn on retrieved address Geometry was not in the expected range.");

            // TODO_PANOS: Fix this! It is currently not fetching the PostcodeGeometry from the database. 
            //Assert.IsNotNull(retrievedAddress.PostcodeGeometry, "PostcodeGeometry field on retrieved address is null");
            //Assert.AreEqual(postcodeGeometry.Latitude, retrievedAddress.PostcodeGeometry.Latitude,
            //    "Field Latitude on PostcodeGeometry of the retrieved address is not the expected.");
            //Assert.AreEqual(postcodeGeometry.Longitude, retrievedAddress.PostcodeGeometry.Longitude,
            //    "Field Longitude on PostcodeGeometry of the retrieved address is not the expected.");

            // I test the GetGeometry method here as I have already set up the data.
            var retrievedAddressGeometry = await serviceForGet.GetGeometry(addressForm.UniqueId);
            Assert.IsNotNull(retrievedAddressGeometry, "Retrieved AddressGeometry is null.");
            Assert.AreEqual(addressGeometry.Latitude, retrievedAddressGeometry.latitude,
                "Field latitude on retrieved AddressGeometry is not the expected.");
            Assert.AreEqual(addressGeometry.Longitude, retrievedAddressGeometry.longitude,
                "Field longitude on retrieved AddressGeometry is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportNortheastLatitude, retrievedAddressGeometry.viewportNortheastLatitude,
                "Field viewportNortheastLatitude on retrieved AddressGeometry is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportNortheastLongitude, retrievedAddressGeometry.viewportNortheastLongitude,
                "Field viewportNortheastLongitude on retrieved AddressGeometry is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportSouthwestLatitude, retrievedAddressGeometry.viewportSouthwestLatitude,
                "Field viewportSouthwestLatitude on retrieved AddressGeometry is not the expected.");
            Assert.AreEqual(addressGeometry.ViewportSouthwestLongitude, retrievedAddressGeometry.viewportSouthwestLongitude,
                "Field viewportSouthwestLatitude on retrieved AddressGeometry is not the expected.");
        }

        [Test]
        public async Task AddAddress_RejectedByAntiAbuseService()
        {
            var ipAddress = "1.2.3.4";
            var countryId = "GB";
            var rejectionReason = "AntiAbuseService Rejection Reason";

            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;
            AddressForm addressFormUsedInVerification = null;
            var userIdUsedInVerification = string.Empty;
            var ipAddressUsedInVerification = string.Empty;

            var containerForAdd = CreateContainer();
            SetupAntiAbuseServiceResponse(containerForAdd, (userId, ipAddr) =>
                {
                    userIdUsedInAntiAbuse = userId;
                    ipAddressUsedInAntiAbuse = ipAddr;
                }, new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = rejectionReason
                });
            SetupAddressVerficationServiceResponse(containerForAdd, (userId, userIpAddress, form) => {
                    userIdUsedInVerification = userId;
                    ipAddressUsedInVerification = userIpAddress;
                    addressFormUsedInVerification = form;
                },
                new AddressVerificationResponse
                {
                    IsRejected = false
                });
            var service = containerForAdd.Get<IAddressService>();

            var addressForm = CreateRandomAddresForm(countryId);
            var outcome = await service.AddAddress(user.Id, ipAddress, addressForm);

            Assert.IsTrue(outcome.IsRejected, "IsRejected on the outcome should be true.");
            Assert.IsNull(outcome.AddressUniqueId, "The AddressId on the outcome should be null.");
            Assert.AreEqual(rejectionReason, outcome.RejectionReason, "The rejection reason is not the expected.");

            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, "UserId used in AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, "IpAddress used in AntiAbuseService is not the expected");
            Assert.IsNull(addressFormUsedInVerification, "Address form should not be submitted for verification.");
            Assert.IsEmpty(userIdUsedInVerification, "UserId used in Verification is not the expected.");
            Assert.IsEmpty(ipAddressUsedInVerification, "IpAddress used in Verification is not the expected.");

            var containerForGet = CreateContainer();
            var serviceForGet = containerForGet.Get<IAddressService>();

            var retrievedAddress = await serviceForGet.GetAddress(addressForm.UniqueId);

            Assert.IsNull(retrievedAddress, "No Address should be created.");
        }

        [Test]
        public async Task AddAddress_RejectedByVerificationService()
        {

            var ipAddress = "1.2.3.4";
            var countryId = "GB";
            var rejectionReason = "VerficiationService Rejection Reason";

            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;
            AddressForm addressFormUsedInVerification = null;
            var userIdUsedInVerification = string.Empty;
            var ipAddressUsedInVerification = string.Empty;

            var containerForAdd = CreateContainer();
            SetupAntiAbuseServiceResponse(containerForAdd, (userId, ipAddr) =>
                {
                    userIdUsedInAntiAbuse = userId;
                    ipAddressUsedInAntiAbuse = ipAddr;
                }, new AntiAbuseServiceResponse
                {
                    IsRejected = false
                });
            SetupAddressVerficationServiceResponse(containerForAdd, (userId, userIpAddress, form) => {
                    userIdUsedInVerification = userId;
                    ipAddressUsedInVerification = userIpAddress;
                    addressFormUsedInVerification = form;
                },
                new AddressVerificationResponse
                {
                    IsRejected = true,
                    RejectionReason = rejectionReason
                });
            var service = containerForAdd.Get<IAddressService>();

            var addressForm = CreateRandomAddresForm(countryId);
            var outcome = await service.AddAddress(user.Id, ipAddress, addressForm);

            Assert.IsTrue(outcome.IsRejected, "IsRejected on the outcome should be true.");
            Assert.IsNull(outcome.AddressUniqueId, "The AddressId on the outcome should be null.");
            Assert.AreEqual(rejectionReason, outcome.RejectionReason, "The rejection reason is not the expected.");

            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, "UserId used in AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, "IpAddress used in AntiAbuseService is not the expected");
            Assert.IsNotNull(addressFormUsedInVerification, "Address form used in VerificationService is null.");
            Assert.AreEqual(addressForm.UniqueId, addressFormUsedInVerification.UniqueId,
                "AddressId on the form submitted for verification is not the expected.");
            Assert.AreEqual(user.Id, userIdUsedInVerification, "UserId used in Verification is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInVerification, "IpAddress used in Verification is not the expected.");

            var containerForGet = CreateContainer();
            var serviceForGet = containerForGet.Get<IAddressService>();

            var retrievedAddress = await serviceForGet.GetAddress(addressForm.UniqueId);

            Assert.IsNull(retrievedAddress, "No Address should be created.");
        }

        #endregion

        #region Private helper functions

        private async Task<PostcodeGeometry> CreatePostcodeGeometry(IKernel container, string countryId, string postcode)
        {
            var addressCleansingHelper = container.Get<IAddressCleansingHelper>();
            var dbContext = container.Get<IEpsilonContext>();
            var postcodeGeometry = new PostcodeGeometry()
            {
                CountryId = countryId,
                Postcode = postcode,
                Latitude = 10.0,
                Longitude = 11.0,
                ViewportNortheastLatitude = 12.0,
                ViewportNortheastLongitude = 13.0,
                ViewportSouthwestLatitude = 14.0,
                ViewportSouthwestLongitude = 15.0
            };
            dbContext.PostcodeGeometries.Add(postcodeGeometry);
            await dbContext.SaveChangesAsync();
            return postcodeGeometry;
        }

        private AddressForm CreateRandomAddresForm(string countryId)
        {
            var random = new RandomWrapper();
            var randomFieldLength = 10;

            var form = new AddressForm
            {
                UniqueId = Guid.NewGuid(),
                Line1 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line2 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line3 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line4 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Locality = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Region = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Postcode = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                CountryId = countryId
            };

            return form;
        }

        private async Task<IList<Address>> CreateAddresses(IKernel container, int count,
            string fieldPrefix, string locality, string postcode, string countryId)
        {
            var random = new RandomWrapper();
            var randomFieldLength = 10;
            var email = String.Format("{0}@test.com", RandomStringHelper.GetString(random, randomFieldLength, RandomStringHelper.CharacterCase.Lower));
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(container, email, ipAddress);
            var addresses = new List<Address>();
            var postcodeGeometry = new PostcodeGeometry()
            {
                CountryId = countryId,
                Postcode = postcode,
                Latitude = 0.0,
                Longitude = 0.0,
                ViewportNortheastLatitude = 0.0,
                ViewportNortheastLongitude = 0.0,
                ViewportSouthwestLatitude = 0.0,
                ViewportSouthwestLongitude = 0.0
            };
            for (int i = 0; i < count; i++)
            {
                var address = new Address
                {
                    UniqueId = Guid.NewGuid(),
                    Line1 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Line2 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Line3 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Line4 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Locality = locality,
                    Region = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Postcode = postcode,
                    CountryId = countryId,
                    CreatedById = user.Id,
                    CreatedByIpAddress = ipAddress,
                    PostcodeGeometry = postcodeGeometry
                };
                addresses.Add(address);
            }
            var dbContext = container.Get<IEpsilonContext>();
            dbContext.Addresses.AddRange(addresses);
            await dbContext.SaveChangesAsync();
            return addresses;
        }

        private void SetupConfig(IKernel container, int searchAddressResultsLimit)
        {
            var mockConfig = new Mock<IAddressServiceConfig>();
            mockConfig.Setup(x => x.SearchAddressResultsLimit).Returns(searchAddressResultsLimit);

            container.Rebind<IAddressServiceConfig>().ToConstant(mockConfig.Object);
        }

        private void SetupAntiAbuseServiceResponse(IKernel container, Action<string, string> callback, 
            AntiAbuseServiceResponse response)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();
            mockAntiAbuseService.Setup(x => x.CanAddAddress(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(callback)
                .Returns(Task.FromResult(response));

            container.Rebind<IAntiAbuseService>().ToConstant(mockAntiAbuseService.Object);
        }

        private void SetupAddressVerficationServiceResponse(IKernel container, Action<string, string, AddressForm> callback,
            AddressVerificationResponse response)
        {
            var mockAddressVerificationService = new Mock<IAddressVerificationService>();
            mockAddressVerificationService.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AddressForm>()))
                .Callback(callback)
                .Returns(Task.FromResult(response));

            container.Rebind<IAddressVerificationService>().ToConstant(mockAddressVerificationService.Object);
        }

        #endregion
    }
}
