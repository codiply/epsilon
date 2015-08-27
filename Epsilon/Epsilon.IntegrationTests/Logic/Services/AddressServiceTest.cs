using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AddressServiceTest : BaseIntegrationTestWithRollback
    {
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
            CountryId? countryIdUsedInAntiAbuse = null;
            AddressForm addressFormUsedInVerification = null;
            var userIdUsedInVerification = string.Empty;
            var ipAddressUsedInVerification = string.Empty;

            var containerForAdd = CreateContainer();
            SetupAntiAbuseServiceResponse(containerForAdd, (userId, ipAddr, cId) =>
            {
                userIdUsedInAntiAbuse = userId;
                ipAddressUsedInAntiAbuse = ipAddr;
                countryIdUsedInAntiAbuse = cId;
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

            var addressForm = CreateRandomAddresForm(countryId, Guid.NewGuid());
            var postcodeGeometry = await CreatePostcodeGeometry(helperContainer, addressForm.CountryId, addressForm.Postcode.ToUpperInvariant());
            Assert.IsNotNull(postcodeGeometry, "PostcodeGeometry just created is null.");

            var timeBefore = DateTimeOffset.Now;
            var outcome = await serviceForAdd.AddAddress(user.Id, ipAddress, addressForm);

            Assert.IsFalse(outcome.IsRejected, "IsRejected on the outcome should be false.");
            Assert.AreEqual(addressForm.UniqueId, outcome.AddressUniqueId, "The AddressId on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The rejection reason should not be populated.");
            var timeAfter = DateTimeOffset.Now;

            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, "UserId used in AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, "IpAddress used in AntiAbuseService is not the expected");
            Assert.AreEqual(EnumsHelper.CountryId.Parse(countryId), countryIdUsedInAntiAbuse,
                "CountryId used in AntiAbuseService is not the expectd.");
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

            Assert.IsNotNull(retrievedAddress.PostcodeGeometry, "PostcodeGeometry field on retrieved address is null");
            Assert.AreEqual(postcodeGeometry.Latitude, retrievedAddress.PostcodeGeometry.Latitude,
                "Field Latitude on PostcodeGeometry of the retrieved address is not the expected.");
            Assert.AreEqual(postcodeGeometry.Longitude, retrievedAddress.PostcodeGeometry.Longitude,
                "Field Longitude on PostcodeGeometry of the retrieved address is not the expected.");

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

            // I try to add a second address using the same UniqueId
            var addressGeometry2 = new AddressGeometry
            {
                Latitude = 11.0,
                Longitude = 12.0,
                ViewportNortheastLatitude = 13.0,
                ViewportNortheastLongitude = 14.0,
                ViewportSouthwestLatitude = 15.0,
                ViewportSouthwestLongitude = 16.0
            };
            var containerForSecondAdd = CreateContainer();
            SetupAntiAbuseServiceResponse(containerForSecondAdd, (userId, ipAddr, cId) => { }, new AntiAbuseServiceResponse
            {
                IsRejected = false
            });
            SetupAddressVerficationServiceResponse(containerForSecondAdd, (userId, userIpAddress, form) => { },
                new AddressVerificationResponse
                {
                    IsRejected = false,
                    AddressGeometry = addressGeometry2
                });
            var serviceForSecondAdd = containerForSecondAdd.Get<IAddressService>();

            var addressForm2 = CreateRandomAddresForm(countryId, addressForm.UniqueId);
            var postcodeGeometry2 = await CreatePostcodeGeometry(helperContainer, addressForm2.CountryId, addressForm2.Postcode);

            Assert.Throws<DbUpdateException>(async () => await serviceForSecondAdd.AddAddress(user.Id, ipAddress, addressForm),
                "Adding a second address using the same UniqueId should throw as there should be a unique constraint on UniqueId.");
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
            CountryId? countryIdUsedInAntiAbuse = null;
            AddressForm addressFormUsedInVerification = null;
            var userIdUsedInVerification = string.Empty;
            var ipAddressUsedInVerification = string.Empty;

            var containerForAdd = CreateContainer();
            SetupAntiAbuseServiceResponse(containerForAdd, (userId, ipAddr, cId) =>
            {
                userIdUsedInAntiAbuse = userId;
                ipAddressUsedInAntiAbuse = ipAddr;
                countryIdUsedInAntiAbuse = cId;
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

            var addressForm = CreateRandomAddresForm(countryId, Guid.NewGuid());
            var outcome = await service.AddAddress(user.Id, ipAddress, addressForm);

            Assert.IsTrue(outcome.IsRejected, "IsRejected on the outcome should be true.");
            Assert.IsNull(outcome.AddressUniqueId, "The AddressId on the outcome should be null.");
            Assert.AreEqual(rejectionReason, outcome.RejectionReason, "The rejection reason is not the expected.");

            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, "UserId used in AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, "IpAddress used in AntiAbuseService is not the expected");
            Assert.AreEqual(EnumsHelper.CountryId.Parse(countryId), countryIdUsedInAntiAbuse,
                "CountryId used in AntiAbuseService is not the expectd.");
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
            CountryId? countryIdUsedInAntiAbuse = null;
            AddressForm addressFormUsedInVerification = null;
            var userIdUsedInVerification = string.Empty;
            var ipAddressUsedInVerification = string.Empty;

            var containerForAdd = CreateContainer();
            SetupAntiAbuseServiceResponse(containerForAdd, (userId, ipAddr, cId) =>
            {
                userIdUsedInAntiAbuse = userId;
                ipAddressUsedInAntiAbuse = ipAddr;
                countryIdUsedInAntiAbuse = cId;
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

            var addressForm = CreateRandomAddresForm(countryId, Guid.NewGuid());
            var outcome = await service.AddAddress(user.Id, ipAddress, addressForm);

            Assert.IsTrue(outcome.IsRejected, "IsRejected on the outcome should be true.");
            Assert.IsNull(outcome.AddressUniqueId, "The AddressId on the outcome should be null.");
            Assert.AreEqual(rejectionReason, outcome.RejectionReason, "The rejection reason is not the expected.");

            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, "UserId used in AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, "IpAddress used in AntiAbuseService is not the expected.");
            Assert.AreEqual(EnumsHelper.CountryId.Parse(countryId), countryIdUsedInAntiAbuse,
                "CountryId used in AntiAbuseService is not the expectd.");
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

        #region AddressHasCompleteSubmissions

        [Test]
        public async Task AddressHasCompleteSubmissions_Test()
        {
            var addressesToCreate = 4;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, random, addressesToCreate, 0, "", locality, postcode, countryId);

            var address1 = addresses[0];
            var submission1 = await CreateSubmissions(helperContainer, random, address1, 0, 0);
            var address2 = addresses[1];
            var submission2 = await CreateSubmissions(helperContainer, random, address2, 0, 1);
            var address3 = addresses[2];
            var submission3 = await CreateSubmissions(helperContainer, random, address3, 1, 0);
            var address4 = addresses[3];
            var submission4 = await CreateSubmissions(helperContainer, random, address4, 1, 1);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IAddressService>();

            var response1 = await service.AddressHasCompleteSubmissions(address1.UniqueId);
            Assert.IsFalse(response1, "Response1 is not the expected.");
            var response2 = await service.AddressHasCompleteSubmissions(address2.UniqueId);
            Assert.IsFalse(response2, "Response2 is not the expected.");
            var response3 = await service.AddressHasCompleteSubmissions(address3.UniqueId);
            Assert.IsTrue(response3, "Response3 is not the expected.");
            var response4 = await service.AddressHasCompleteSubmissions(address4.UniqueId);
            Assert.IsTrue(response4, "Response4 is not the expected.");

            var responseForNonExistiningAddress = await service.AddressHasCompleteSubmissions(Guid.NewGuid());
            Assert.IsFalse(responseForNonExistiningAddress, "Response for non-existing address shoud be false.");
        }

        #endregion

        #region GetAddress

        [Test]
        public async Task GetAddress_Test()
        {
            var addressesToCreate = 5;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, random, addressesToCreate, 0, "", locality, postcode, countryId);

            var expectedAddress = addresses.First();

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IAddressService>();

            var actualAddress = await service.GetAddress(expectedAddress.UniqueId);

            Assert.IsNotNull(actualAddress, "The address was not found.");
            Assert.AreEqual(expectedAddress.Line1, actualAddress.Line1, "Line1 is not the expected.");
            Assert.AreEqual(expectedAddress.Line2, actualAddress.Line2, "Line2 is not the expected.");
            Assert.AreEqual(expectedAddress.Line3, actualAddress.Line3, "Line3 is not the expected.");
            Assert.AreEqual(expectedAddress.Line4, actualAddress.Line4, "Line4 is not the expected.");
            Assert.AreEqual(expectedAddress.Locality, actualAddress.Locality, "Locality is not the expected.");
            Assert.AreEqual(expectedAddress.Region, actualAddress.Region, "Region is not the expected.");
            Assert.AreEqual(expectedAddress.Postcode, actualAddress.Postcode, "Postcode is not the expected.");
            Assert.AreEqual(expectedAddress.CountryId, actualAddress.CountryId, "CountryId is not the expected.");

            var nonExistingAddress = await service.GetAddress(Guid.NewGuid());
            Assert.IsNull(nonExistingAddress, "When using a UniqueId that does not exist, the returned address should be null.");
        }

        #endregion

        #region GetAddress

        [Test]
        public async Task GetAddressWithGeometries_Test()
        {
            var addressesToCreate = 5;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, random, addressesToCreate, 0, "", locality, postcode, countryId);

            var expectedAddress = addresses.First();

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IAddressService>();

            var actualAddress = await service.GetAddressWithGeometries(expectedAddress.UniqueId);

            Assert.IsNotNull(actualAddress, "The address was not found.");

            Assert.IsNotNull(actualAddress.Geometry, "Geometry should not be null.");
            Assert.AreEqual(expectedAddress.Geometry.Latitude, actualAddress.Geometry.Latitude,
                "Geometry.Latitude is not the expected.");
            Assert.AreEqual(expectedAddress.Geometry.Longitude, actualAddress.Geometry.Longitude,
                "Geometry.Latitude is not the expected.");

            Assert.IsNotNull(actualAddress.PostcodeGeometry, "PostcodeGeometry should not be null.");
            Assert.AreEqual(expectedAddress.PostcodeGeometry.Latitude, actualAddress.PostcodeGeometry.Latitude,
                "PostcodeGeometry.Latitude is not the expected.");
            Assert.AreEqual(expectedAddress.PostcodeGeometry.Longitude, actualAddress.PostcodeGeometry.Longitude,
                "PostcodeGeometry.Latitude is not the expected.");


            var nonExistingAddress = await service.GetAddressWithGeometries(Guid.NewGuid());
            Assert.IsNull(nonExistingAddress, "When using a UniqueId that does not exist, the returned address should be null.");
        }

        #endregion

        #region GetGeometry

        [Test]
        public async Task GetGeometry_Test()
        {
            var addressesToCreate = 5;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, random, addressesToCreate, 0, "", locality, postcode, countryId);

            var expectedAddress = addresses.First();

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IAddressService>();

            var actualGeometry = await service.GetGeometry(expectedAddress.UniqueId);

            Assert.IsNotNull(actualGeometry, "The geometry was not found.");
            Assert.AreEqual(expectedAddress.Geometry.Latitude, actualGeometry.latitude, "Latitude is not the expected.");
            Assert.AreEqual(expectedAddress.Geometry.Longitude, actualGeometry.longitude, "Longitude is not the expected.");


            var geometryForNonExistingAddress = await service.GetAddress(Guid.NewGuid());
            Assert.IsNull(geometryForNonExistingAddress, "When using a UniqueId that does not exist, the returned geometry should be null.");
        }

        #endregion

        #region SearchAddress

        [Test]
        public async Task SearchAddress_ExactPostcode_AndResultsCountEqualToLimit()
        {
            int searchAddressResultsLimit = 10;
            int numberOfAddressesToCreate = searchAddressResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit: searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(
                helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.SearchAddress(request1);

            Assert.AreEqual(addresses.Count, response1.results.Count,
                "A request with exact postcode (case-insensitive) should match all the addresses.");

            var request2 = new AddressSearchRequest { countryId = countryId, postcode = "POSTCO", terms = "" };
            var response2 = await serviceUnderTest.SearchAddress(request2);

            Assert.IsEmpty(response2.results, "A request with part of the postcode should return no results.");
            Assert.IsFalse(response2.isResultsLimitExceeded, 
                "The results limit should not be flagged as exceeded when the number of results is equal to the limit.");
            Assert.AreEqual(searchAddressResultsLimit, response2.resultsLimit, 
                "The results limit reported on the response was not the expected."); 
        }

        [Test]
        public async Task SearchAddress_SearchesTermsInAllFields()
        {
            int searchAddressResultsLimit = 3;
            int numberOfAddressesToCreate = searchAddressResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit: searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.SearchAddress(request1);

            var addressToSearch = addresses.First();

            // I use each field as a term and separate them in different ways in the search terms string.
            var searchTermsField = string.Format(" {0},{1}, {2} ,{3} {4}   {5}, ",
                addressToSearch.Line1, addressToSearch.Line2, addressToSearch.Line3, addressToSearch.Line4,
                addressToSearch.Locality, addressToSearch.Region);

            var request = new AddressSearchRequest { countryId = countryId, postcode = postcode, terms = searchTermsField };
            var response = await serviceUnderTest.SearchAddress(request);

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
        public async Task SearchAddress_DoesNotUseSearchTermsToSearchCountryIdOrPostcode()
        {
            int searchAddressResultsLimit = 3;
            int numberOfAddressesToCreate = searchAddressResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit: searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.SearchAddress(request1);

            var addressToSearch = addresses.First();

            var searchTermsField = string.Format("{0} {1}", postcode, countryId);

            var request = new AddressSearchRequest { countryId = countryId, postcode = postcode, terms = searchTermsField };
            var response = await serviceUnderTest.SearchAddress(request);

            Assert.IsEmpty(response.results, "The response should contain no results.");
            Assert.IsFalse(response.isResultsLimitExceeded, "The results limit should not be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.resultsLimit,
                "The results limit on the response was not the expected.");
        }

        [Test]
        public async Task SearchAddress_ReturnsNoMoreAddressesThanTheLimit()
        {
            int searchAddressResultsLimit = 3;
            int numberOfAddressesToCreate = searchAddressResultsLimit + 1;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit: searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response = await serviceUnderTest.SearchAddress(request);

            Assert.AreEqual(searchAddressResultsLimit, response.results.Count,
                "The number of addresses returned should equal the results limit.");
            Assert.IsTrue(response.isResultsLimitExceeded,
                "The results limit should be flagged as exceeded.");
            Assert.AreEqual(searchAddressResultsLimit, response.resultsLimit,
                "The results limit reported on the response was not the expected.");
        }

        [Test]
        public async Task SearchAddress_DoesNotReturnHiddenAddresses()
        {
            int searchAddressResultsLimit = 10;
            int numberOfNonHiddenAddressesToCreate = 5;
            int numberOfHiddenAddressesToCreate = 5;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchAddressResultsLimit: searchAddressResultsLimit);

            var helperContainer = CreateContainer();
            var addresses = await CreateAddresses(
                helperContainer, random, numberOfNonHiddenAddressesToCreate, numberOfHiddenAddressesToCreate, "", locality, postcode, countryId);

            var nonHiddenAddresses = addresses.Where(x => !x.IsHidden).ToList();

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request = new AddressSearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response = await serviceUnderTest.SearchAddress(request);

            Assert.AreEqual(numberOfNonHiddenAddressesToCreate, response.results.Count,
                "The number of addresses returned should equal the number of non-hidden addresses.");
            foreach (var address in nonHiddenAddresses)
            {
                var addressFoundOnResponse = response.results.SingleOrDefault(x => x.addressUniqueId.Equals(address.UniqueId));
                Assert.IsNotNull(addressFoundOnResponse,
                    string.Format("Non-hidden address '{0}' was not found on the response.", address.FullAddress()));
            }
        }

        #endregion

        #region SearchProperty

        [Test]
        public async Task SearchProperty_ExactPostcode_AndResultsCountEqualToLimit()
        {
            int searchPropertyResultsLimit = 10;
            int numberOfAddressesToCreate = searchPropertyResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchPropertyResultsLimit: searchPropertyResultsLimit);

            var helperContainer = CreateContainer();
            var properties = await CreateAddresses(
                helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new PropertySearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.SearchProperty(request1);

            Assert.AreEqual(properties.Count, response1.results.Count,
                "A request with exact postcode (case-insensitive) should match all the properties.");

            var request2 = new PropertySearchRequest { countryId = countryId, postcode = "POSTCO", terms = "" };
            var response2 = await serviceUnderTest.SearchProperty(request2);

            Assert.IsEmpty(response2.results, "A request with part of the postcode should return no results.");
            Assert.IsFalse(response2.isResultsLimitExceeded,
                "The results limit should not be flagged as exceeded when the number of results is equal to the limit.");
            Assert.AreEqual(searchPropertyResultsLimit, response2.resultsLimit,
                "The results limit reported on the response was not the expected.");
        }

        [Test]
        public async Task SearchProperty_SearchesTermsInAllFields()
        {
            int searchPropertyResultsLimit = 3;
            int numberOfAddressesToCreate = searchPropertyResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchPropertyResultsLimit: searchPropertyResultsLimit);

            var helperContainer = CreateContainer();
            var properties = await CreateAddresses(helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new PropertySearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.SearchProperty(request1);

            var propertyToSearch = properties.First();

            // I use each field as a term and separate them in different ways in the search terms string.
            var searchTermsField = string.Format(" {0},{1}, {2} ,{3} {4}   {5}, ",
                propertyToSearch.Line1, propertyToSearch.Line2, propertyToSearch.Line3, propertyToSearch.Line4,
                propertyToSearch.Locality, propertyToSearch.Region);

            var request = new PropertySearchRequest { countryId = countryId, postcode = postcode, terms = searchTermsField };
            var response = await serviceUnderTest.SearchProperty(request);

            Assert.AreEqual(1, response.results.Count, "The response should contain a single result.");
            Assert.IsFalse(response.isResultsLimitExceeded, "The results limit should not be flagged as exceeded.");
            Assert.AreEqual(searchPropertyResultsLimit, response.resultsLimit,
                "The results limit on the response was not the expected.");

            var returnedAddress = response.results.Single();

            Assert.AreEqual(propertyToSearch.UniqueId, returnedAddress.addressUniqueId, "The id of the address returned is not the expected.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(propertyToSearch.Line1), "Line1 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(propertyToSearch.Line2), "Line2 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(propertyToSearch.Line3), "Line3 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(propertyToSearch.Line4), "Line4 was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(propertyToSearch.Locality), "Locality was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(propertyToSearch.Region), "Region was not found in the full address.");
            Assert.IsTrue(returnedAddress.fullAddress.Contains(propertyToSearch.Postcode), "Postcode was not found in the full address.");
        }

        [Test]
        public async Task SearchProperty_DoesNotUseSearchTermsToSearchCountryIdOrPostcode()
        {
            int searchPropertyResultsLimit = 3;
            int numberOfAddressesToCreate = searchPropertyResultsLimit;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchPropertyResultsLimit: searchPropertyResultsLimit);

            var helperContainer = CreateContainer();
            var properties = await CreateAddresses(helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request1 = new PropertySearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response1 = await serviceUnderTest.SearchProperty(request1);

            var propertyToSearch = properties.First();

            var searchTermsField = string.Format("{0} {1}", postcode, countryId);

            var request = new PropertySearchRequest { countryId = countryId, postcode = postcode, terms = searchTermsField };
            var response = await serviceUnderTest.SearchProperty(request);

            Assert.IsEmpty(response.results, "The response should contain no results.");
            Assert.IsFalse(response.isResultsLimitExceeded, "The results limit should not be flagged as exceeded.");
            Assert.AreEqual(searchPropertyResultsLimit, response.resultsLimit,
                "The results limit on the response was not the expected.");
        }

        [Test]
        public async Task SearchProperty_ReturnsNoMoreAddressesThanTheLimit()
        {
            int searchPropertyResultsLimit = 3;
            int numberOfAddressesToCreate = searchPropertyResultsLimit + 1;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchPropertyResultsLimit: searchPropertyResultsLimit);

            var helperContainer = CreateContainer();
            var properties = await CreateAddresses(helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request = new PropertySearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response = await serviceUnderTest.SearchProperty(request);

            Assert.AreEqual(searchPropertyResultsLimit, response.results.Count,
                "The number of addresses returned should equal the results limit.");
            Assert.IsTrue(response.isResultsLimitExceeded,
                "The results limit should be flagged as exceeded.");
            Assert.AreEqual(searchPropertyResultsLimit, response.resultsLimit,
                "The results limit reported on the response was not the expected.");
        }

        [Test]
        public async Task SearchProperty_DoesNotReturnHiddenAddresses()
        {
            int searchPropertyResultsLimit = 10;
            int numberOfNonHiddenAddressesToCreate = 5;
            int numberOfHiddenAddressesToCreate = 5;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchPropertyResultsLimit: searchPropertyResultsLimit);

            var helperContainer = CreateContainer();
            var properties = await CreateAddresses(
                helperContainer, random, numberOfNonHiddenAddressesToCreate, numberOfHiddenAddressesToCreate, "", locality, postcode, countryId);

            var nonHiddenProperties = properties.Where(x => !x.IsHidden).ToList();

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request = new PropertySearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response = await serviceUnderTest.SearchProperty(request);

            Assert.AreEqual(numberOfNonHiddenAddressesToCreate, response.results.Count,
                "The number of addresses returned should equal the number of non-hidden properties.");
            foreach (var property in nonHiddenProperties)
            {
                var propertyFoundOnResponse = response.results.SingleOrDefault(x => x.addressUniqueId.Equals(property.UniqueId));
                Assert.IsNotNull(propertyFoundOnResponse,
                    string.Format("Non-hidden property '{0}' was not found on the response.", property.FullAddress()));
            }
        }

        [Test]
        public async Task SearchProperty_ReturnsCorrectSubmissionInformation()
        {
            int searchPropertyResultsLimit = 10;
            int numberOfAddressesToCreate = 4;
            var locality = "Locality";
            var postcode = "POSTCODE";
            var countryId = "GB";

            var random = new RandomWrapper(2015);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, searchPropertyResultsLimit: searchPropertyResultsLimit);

            var helperContainer = CreateContainer();
            var properties = await CreateAddresses(
                helperContainer, random, numberOfAddressesToCreate, 0, "", locality, postcode, countryId);

            var property1 = properties[0];
            var submissions1 = await CreateSubmissions(helperContainer, random, property1, 0, 0);
            var completeSubmissions1 = submissions1.Where(x => x.SubmittedOn.HasValue).ToList();
            var property2 = properties[1];
            var submissions2 = await CreateSubmissions(helperContainer, random, property2, 0, 2);
            var completeSubmissions2 = submissions2.Where(x => x.SubmittedOn.HasValue).ToList();
            var property3 = properties[2];
            var submissions3 = await CreateSubmissions(helperContainer, random, property3, 1, 1);
            var completeSubmissions3 = submissions3.Where(x => x.SubmittedOn.HasValue).ToList();
            var property4 = properties[3];
            var submissions4 = await CreateSubmissions(helperContainer, random, property4, 3, 2);
            var completeSubmissions4 = submissions4.Where(x => x.SubmittedOn.HasValue).ToList();

            var serviceUnderTest = containerUnderTest.Get<IAddressService>();

            var request = new PropertySearchRequest { countryId = countryId, postcode = postcode.ToLower(), terms = "" };
            var response = await serviceUnderTest.SearchProperty(request);


            // Property 1
            var property1OnResponse = response.results.SingleOrDefault(x => x.addressUniqueId.Equals(property1.UniqueId));
            Assert.IsNotNull(property1OnResponse, "Property 1 was not found on the response.");
            Assert.AreEqual(completeSubmissions1.Count, property1OnResponse.numberOfCompletedSubmissions,
                "The number of complete submissions is not the expected for property 1.");
            Assert.IsNull(property1OnResponse.lastSubmissionOn, "LastSubmissionOn is not the expected for property 1.");

            // Property 2
            var property2OnResponse = response.results.SingleOrDefault(x => x.addressUniqueId.Equals(property2.UniqueId));
            Assert.IsNotNull(property2OnResponse, "Property 2 was not found on the response.");
            Assert.AreEqual(completeSubmissions2.Count, property2OnResponse.numberOfCompletedSubmissions,
                "The number of complete submissions is not the expected for property 2.");
            Assert.AreEqual(completeSubmissions2.Select(x => x.SubmittedOn).OrderByDescending(x => x).FirstOrDefault(), 
                property2OnResponse.lastSubmissionOn, "LastSubmissionOn is not the expected for property 2.");

            // Property 3
            var property3OnResponse = response.results.SingleOrDefault(x => x.addressUniqueId.Equals(property3.UniqueId));
            Assert.IsNotNull(property3OnResponse, "Property 3 was not found on the response.");
            Assert.AreEqual(completeSubmissions3.Count, property3OnResponse.numberOfCompletedSubmissions,
                "The number of complete submissions is not the expected for property 3.");
            Assert.AreEqual(completeSubmissions3.Select(x => x.SubmittedOn).OrderByDescending(x => x).FirstOrDefault(),
                property3OnResponse.lastSubmissionOn, "LastSubmissionOn is not the expected for property 3.");

            // Property 4
            var property4OnResponse = response.results.SingleOrDefault(x => x.addressUniqueId.Equals(property4.UniqueId));
            Assert.IsNotNull(property1OnResponse, "Property 4 was not found on the response.");
            Assert.AreEqual(completeSubmissions4.Count, property4OnResponse.numberOfCompletedSubmissions,
                "The number of complete submissions is not the expected for property 4.");
            Assert.AreEqual(completeSubmissions4.Select(x => x.SubmittedOn).OrderByDescending(x => x).FirstOrDefault(),
                property4OnResponse.lastSubmissionOn, "LastSubmissionOn is not the expected for property 4.");
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

        private AddressForm CreateRandomAddresForm(string countryId, Guid uniqueId)
        {
            var random = new RandomWrapper();
            var randomFieldLength = 10;

            var form = new AddressForm
            {
                UniqueId = uniqueId,
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

        private async Task<IList<Address>> CreateAddresses(
            IKernel container, IRandomWrapper random, int nonHiddenAddresses, int hiddenAddresses,
            string fieldPrefix, string locality, string postcode, string countryId)
        {
            var randomFieldLength = 10;
            var email = String.Format("{0}@test.com", RandomStringHelper.GetString(random, randomFieldLength, RandomStringHelper.CharacterCase.Lower));
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(container, email, ipAddress);
            var addresses = new List<Address>();
            var postcodeGeometry = new PostcodeGeometry()
            {
                CountryId = countryId,
                Postcode = postcode,
                Latitude = random.NextDouble(),
                Longitude = random.NextDouble(),
                ViewportNortheastLatitude = random.NextDouble(),
                ViewportNortheastLongitude = random.NextDouble(),
                ViewportSouthwestLatitude = random.NextDouble(),
                ViewportSouthwestLongitude = random.NextDouble()
            };
            for (int i = 0; i < nonHiddenAddresses; i++)
            {
                var address = CreateRandomAddress(random, fieldPrefix, randomFieldLength,
                    locality, postcode, countryId, user, ipAddress, postcodeGeometry);
                address.IsHidden = false;
                addresses.Add(address);
            }
            for (int i = 0; i < hiddenAddresses; i++)
            {
                var address = CreateRandomAddress(random, fieldPrefix, randomFieldLength,
                    locality, postcode, countryId, user, ipAddress, postcodeGeometry);
                address.IsHidden = true;
                addresses.Add(address);
            }
            var dbContext = container.Get<IEpsilonContext>();
            dbContext.Addresses.AddRange(addresses);
            await dbContext.SaveChangesAsync();
            return addresses;
        }

        private Address CreateRandomAddress(
            IRandomWrapper random, string fieldPrefix, int randomFieldLength, string locality, string postcode, string countryId,
            User user, string ipAddress, PostcodeGeometry postcodeGeometry)
        {
            var geometry = new AddressGeometry
            {
                Latitude = random.NextDouble(),
                Longitude = random.NextDouble(),
                ViewportNortheastLatitude = random.NextDouble(),
                ViewportNortheastLongitude = random.NextDouble(),
                ViewportSouthwestLatitude = random.NextDouble(),
                ViewportSouthwestLongitude = random.NextDouble(),
            };
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
                Geometry = geometry,
                PostcodeGeometry = postcodeGeometry
            };
            return address;
        }

        private async Task<IList<TenancyDetailsSubmission>> CreateSubmissions(
            IKernel container, IRandomWrapper random, Address address, int numberOfCompleteSubmissions, int numberOfIncompleteSubmissions)
        {
            var randomFieldLength = 10;
            var email = String.Format("{0}@test.com", RandomStringHelper.GetString(random, randomFieldLength, RandomStringHelper.CharacterCase.Lower));
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(container, email, ipAddress);
            var clock = container.Get<IClock>();

            var submissions = new List<TenancyDetailsSubmission>();

            for (var i = 0; i < numberOfCompleteSubmissions; i++)
            {
                var submission = new TenancyDetailsSubmission
                {
                    AddressId = address.Id,
                    UserId = user.Id,
                    CreatedByIpAddress = ipAddress,
                    UniqueId = Guid.NewGuid(),
                    SubmittedOn = clock.OffsetNow,
                    CurrencyId = EnumsHelper.CurrencyId.ToString(CurrencyId.GBP),
                    RentPerMonth = random.Next(100, 1000),
                    IsPartOfProperty = false,
                    IsFurnished = false
                };
                submissions.Add(submission);
            }

            for (var i = 0; i < numberOfIncompleteSubmissions; i++)
            {
                var submission = new TenancyDetailsSubmission
                {
                    AddressId = address.Id,
                    UserId = user.Id,
                    CreatedByIpAddress = ipAddress,
                    UniqueId = Guid.NewGuid()
                };
                submissions.Add(submission);
            }

            var dbContext = container.Get<IEpsilonContext>();
            dbContext.TenancyDetailsSubmissions.AddRange(submissions);
            await dbContext.SaveChangesAsync();

            return submissions;
        }

        private void SetupConfig(IKernel container, 
            bool disableAddAddress = false, int searchAddressResultsLimit = 100, int searchPropertyResultsLimit = 100)
        {
            var mockConfig = new Mock<IAddressServiceConfig>();
            mockConfig.Setup(x => x.GlobalSwitch_DisableAddAddress).Returns(disableAddAddress);
            mockConfig.Setup(x => x.SearchAddressResultsLimit).Returns(searchAddressResultsLimit);
            mockConfig.Setup(x => x.SearchPropertyResultsLimit).Returns(searchPropertyResultsLimit);

            container.Rebind<IAddressServiceConfig>().ToConstant(mockConfig.Object);
        }

        private void SetupAntiAbuseServiceResponse(IKernel container, Action<string, string, CountryId> callback, 
            AntiAbuseServiceResponse response)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();
            mockAntiAbuseService.Setup(x => x.CanAddAddress(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CountryId>()))
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
