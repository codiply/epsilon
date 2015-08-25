using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Ninject;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Moq;
using System.Data.Entity;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Constants;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AddressVerificationServiceTest : BaseIntegrationTestWithRollback
    {
        private readonly TimeSpan DelayBetweenCallsToTheApi = TimeSpan.FromSeconds(0.4);

        public static GeocodePostcodeStatus[] NonSuccessGeocodePostcodeStatuses =
            EnumsHelper.GeocodePostcodeStatus.GetValues().Where(x => x != GeocodePostcodeStatus.Success).ToArray();

        private static GeocodeAddressStatus[] NonSuccessGeocodeAddressStatuses =
            EnumsHelper.GeocodeAddressStatus.GetValues().Where(x => x != GeocodeAddressStatus.Success).ToArray();

        [Test]
        public async Task Verify_HappyPath_GB()
        {
            var container = CreateContainer();
            var service = container.Get<IAddressVerificationService>();

            var ipAddress = "1.2.3.4";
            var user = await CreateUser(CreateContainer(), "test@test.com", ipAddress);

            var countryId = CountryId.GB;
            var addressForm = new AddressForm
            {
                Line1 = "229 Great Portland Street",
                Locality = "London",
                Region = "London",
                Postcode = "W1W5PN",
                CountryId = EnumsHelper.CountryId.ToString(countryId)
            };

            await Task.Delay(DelayBetweenCallsToTheApi);
            var clock = container.Get<IClock>();
            var timeBefore = clock.OffsetNow;
            var response = await service.Verify(user.Id, ipAddress, addressForm);
            var timeAfter = clock.OffsetNow;

            Assert.IsFalse(response.IsRejected, "IsRejected on the response is not the expected.");
            Assert.IsNull(response.RejectionReason, "RejectionReason on the response is not the expected.");
            Assert.IsNotNull(response.AddressGeometry, "AddressGeometry should not be null.");
            Assert.That(response.AddressGeometry.Latitude, Is.EqualTo(51.52361).Within(0.00001),
                "AddressGeometry.Latitude is not the expected.");
            Assert.That(response.AddressGeometry.Longitude, Is.EqualTo(-0.14448).Within(0.00001),
                "AddressGeometry.Longitude is not the expected.");

            var retrievedGeocodeFailure = await DbProbe.GeocodeFailures
                .SingleOrDefaultAsync(x => x.CreatedById.Equals(user.Id));
            Assert.IsNull(retrievedGeocodeFailure, "No GeocodeFailure should be created.");
        }

        [Test, TestCaseSource("NonSuccessGeocodePostcodeStatuses")]
        public async Task Verify_GeocodePostcodeStatusNotSuccess_DoesNotThrowAndSavesGeocodeFailure(
            GeocodePostcodeStatus geocodePostcodeStatus)
        {
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(CreateContainer(), "test@test.com", ipAddress);

            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);
            var addressForm = new AddressForm
            {
                Line1 = " 229 Great Portland Street ",
                Locality = " London ",
                Region = " London ",
                Postcode = " w1w 5pn ",
                CountryId = countryId
            };

            var expectedCleanAddressForm = new AddressForm
            {
                Line1 = "229 Great Portland Street",
                Locality = "London",
                Region = "London",
                Postcode = "W1W5PN",
                CountryId = countryId
            };

            var geocodeAddressResponse = new GeocodeAddressResponse() { Status = GeocodeAddressStatus.Success };

            var container = CreateContainer();
            SetupGeocodeService(container, countryId,
                expectedCleanAddressForm.Postcode, geocodePostcodeStatus,
                expectedCleanAddressForm.FullAddressWithoutCountry(), geocodeAddressResponse);
            var clock = container.Get<IClock>();
            var service = container.Get<IAddressVerificationService>();

            var timeBefore = clock.OffsetNow;
            var response = await service.Verify(user.Id, ipAddress, addressForm);
            var timeAfter = clock.OffsetNow;

            Assert.IsTrue(response.IsRejected, "IsRejected field on response is not the expected.");

            // Failure due to techncal reasons
            if (geocodePostcodeStatus == GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes ||
                geocodePostcodeStatus == GeocodePostcodeStatus.ServiceUnavailable)
            {
                Assert.IsFalse(response.AskUserToModify, "AskUserToModify on the response is not the expected.");
            }
            else
            {
                Assert.IsTrue(response.AskUserToModify, "AskUserToModify on the response is not the expected.");

                var retrievedGeocodeFailure = await DbProbe.GeocodeFailures
                    .SingleOrDefaultAsync(x => x.CreatedById.Equals(user.Id));

                Assert.IsNotNull(retrievedGeocodeFailure, "A GeocodeFailure was not created.");
                Assert.AreEqual(expectedCleanAddressForm.Postcode, retrievedGeocodeFailure.Address,
                    "Address on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(countryId, retrievedGeocodeFailure.CountryId,
                    "CountryId on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(ipAddress, retrievedGeocodeFailure.CreatedByIpAddress,
                    "CreatedByIpAddress on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(EnumsHelper.GeocodePostcodeStatus.ToString(geocodePostcodeStatus), 
                    retrievedGeocodeFailure.FailureType,
                    "FailureType on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(AppConstant.GEOCODE_QUERY_TYPE_POSTCODE,
                    retrievedGeocodeFailure.QueryType,
                    "FailureType on the retrieved GeocodeFailure is not the expected.");
                Assert.That(retrievedGeocodeFailure.CreatedOn, Is.GreaterThanOrEqualTo(timeBefore),
                    "retrievedGeocodeFailure.CreatedOn lower bound test.");
                Assert.That(retrievedGeocodeFailure.CreatedOn, Is.LessThanOrEqualTo(timeAfter),
                    "retrievedGeocodeFailure.CreatedOn upper bound test.");
            }
        }

        [Test, TestCaseSource("NonSuccessGeocodeAddressStatuses")]
        public async Task Verify_GeocodeAddressStatusNotSuccess_DoesNotThrowAndSavesGeocodeFailure(
            GeocodeAddressStatus geocodeAddressStatus)
        {
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(CreateContainer(), "test@test.com", ipAddress);

            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);
            var addressForm = new AddressForm
            {
                Line1 = " 229 Great Portland Street ",
                Locality = " London ",
                Region = " London ",
                Postcode = " w1w 5pn ",
                CountryId = countryId
            };

            var expectedCleanAddressForm = new AddressForm
            {
                Line1 = "229 Great Portland Street",
                Locality = "London",
                Region = "London",
                Postcode = "W1W5PN",
                CountryId = countryId
            };

            var geocodePostcodeStatus = GeocodePostcodeStatus.Success;
            var geocodeAddressResponse = new GeocodeAddressResponse() { Status = geocodeAddressStatus };

            var container = CreateContainer();
            SetupGeocodeService(container, countryId,
                expectedCleanAddressForm.Postcode, geocodePostcodeStatus,
                expectedCleanAddressForm.FullAddressWithoutCountry(), geocodeAddressResponse);
            var clock = container.Get<IClock>();
            var service = container.Get<IAddressVerificationService>();

            var timeBefore = clock.OffsetNow;
            var response = await service.Verify(user.Id, ipAddress, addressForm);
            var timeAfter = clock.OffsetNow;

            Assert.IsTrue(response.IsRejected, "IsRejected field on response is not the expected.");

            // Failure due to techncal reasons
            if (geocodeAddressStatus == GeocodeAddressStatus.OverQueryLimitTriedMaxTimes ||
                geocodeAddressStatus == GeocodeAddressStatus.ServiceUnavailable)
            {
                Assert.IsFalse(response.AskUserToModify, "AskUserToModify on the response is not the expected.");
            }
            else
            {
                Assert.IsTrue(response.AskUserToModify, "AskUserToModify on the response is not the expected.");

                var retrievedGeocodeFailure = await DbProbe.GeocodeFailures
                    .SingleOrDefaultAsync(x => x.CreatedById.Equals(user.Id));

                Assert.IsNotNull(retrievedGeocodeFailure, "A GeocodeFailure was not created.");
                Assert.AreEqual(expectedCleanAddressForm.FullAddressWithoutCountry(), retrievedGeocodeFailure.Address,
                    "Address on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(countryId, retrievedGeocodeFailure.CountryId,
                    "CountryId on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(ipAddress, retrievedGeocodeFailure.CreatedByIpAddress,
                    "CreatedByIpAddress on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(EnumsHelper.GeocodeAddressStatus.ToString(geocodeAddressStatus),
                    retrievedGeocodeFailure.FailureType,
                    "FailureType on the retrieved GeocodeFailure is not the expected.");
                Assert.AreEqual(AppConstant.GEOCODE_QUERY_TYPE_ADDRESS,
                    retrievedGeocodeFailure.QueryType,
                    "FailureType on the retrieved GeocodeFailure is not the expected.");
                Assert.That(retrievedGeocodeFailure.CreatedOn, Is.GreaterThanOrEqualTo(timeBefore),
                    "retrievedGeocodeFailure.CreatedOn lower bound test.");
                Assert.That(retrievedGeocodeFailure.CreatedOn, Is.LessThanOrEqualTo(timeAfter),
                    "retrievedGeocodeFailure.CreatedOn upper bound test.");
            }
        }

        private void SetupGeocodeService(IKernel container,
            string expectedCountryId, string expectedPostcode, GeocodePostcodeStatus geocodePostcodeStatusToReturn,
            string expectedAddress, GeocodeAddressResponse geocodeAddressResponseToReturn)
        {
            var mockGeocodeService = new Mock<IGeocodeService>();

            mockGeocodeService.Setup(x => x.GeocodePostcode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((postcode, countryId) =>
                {
                    if (!postcode.Equals(expectedPostcode))
                        throw new Exception(string.Format("Expected postcode '{0}' but got '{1}' in GeocodeService.GeocodePostcode.",
                            expectedPostcode, postcode));

                    if (!countryId.Equals(expectedCountryId))
                        throw new Exception(string.Format("Expected CountryId '{0}' but got '{1}' in GeocodeService.GeocodePostcode.",
                            expectedCountryId, countryId));

                    return Task.FromResult(geocodePostcodeStatusToReturn);
                });

            mockGeocodeService.Setup(x => x.GeocodeAddress(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((address, countryId) =>
                {
                    if (!address.Equals(expectedAddress))
                        throw new Exception(string.Format("Expected address '{0}' but got '{1}' in GeocodeService.GeocodeAddress.",
                            expectedAddress, address));

                    if (!countryId.Equals(expectedCountryId))
                        throw new Exception(string.Format("Expected CountryId '{0}' but got '{1}' in GeocodeService.GeocodeAddress.",
                            expectedCountryId, countryId));

                    return Task.FromResult(geocodeAddressResponseToReturn);
                });

            container.Rebind<IGeocodeService>().ToConstant(mockGeocodeService.Object);
        }
    }
}