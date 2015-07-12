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

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AddressServiceTest : BaseIntegrationTestWithRollback
    { 
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

            Assert.AreEqual(addresses.Count, response1.Results.Count,
                "A request with exact postcode (case-insensitive) should match all the addresses.");

            var request2 = new AddressSearchRequest { countryId = countryId, postcode = "POSTCO", terms = "" };
            var response2 = await serviceUnderTest.Search(request2);

            Assert.IsEmpty(response2.Results, "A request with part of the postcode should return no results.");
            Assert.IsFalse(response2.IsResultsLimitExceeded, 
                "The results limit should not be flagged as exceeded when the number of results is equal to the limit.");
            Assert.AreEqual(searchAddressResultsLimit, response2.ResultsLimit, 
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

            Assert.AreEqual(1, response.Results.Count, "The response should contain a single result.");
            Assert.IsFalse(response.IsResultsLimitExceeded, "The results limit should not be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.ResultsLimit, 
                "The results limit on the response was not the expected.");

            var returnedAddress = response.Results.Single();

            Assert.AreEqual(addressToSearch.Id, returnedAddress.addressId, "The id of the address returned is not the expected.");
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

            Assert.IsEmpty(response.Results, "The response should contain no results.");
            Assert.IsFalse(response.IsResultsLimitExceeded, "The results limit should not be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.ResultsLimit,
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

            Assert.AreEqual(searchAddressResultsLimit, response.Results.Count,
                "The number of addresses returned should equal the results limit.");
            Assert.IsTrue(response.IsResultsLimitExceeded,
                "The results limit should be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.ResultsLimit,
                "The results limit reported on the response was not the expected.");
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
            for (int i = 0; i < count; i++)
            {
                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    Line1 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Line2 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Line3 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Line4 = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Locality = locality,
                    Region = fieldPrefix + RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                    Postcode = postcode,
                    CountryId = countryId,
                    CreatedById = user.Id,
                    CreatedByIpAddress = ipAddress
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
