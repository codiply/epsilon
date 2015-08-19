using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Logic.AntiAbuse;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class AntiAbuseServiceTest
    {
        #region CanRegister

        [Test]
        public async Task CanRegister_GlobalSwitchWorks()
        {
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();
            mockAntiAbuseServiceConfig.Setup(x => x.GlobalSwitch_DisableRegister).Returns(true);
            var service = new AntiAbuseService(null, mockAntiAbuseServiceConfig.Object, null, null, null, null);

            var outcome = await service.CanRegister("some-ip-address");

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected should be true.");
            Assert.AreEqual(AntiAbuseResources.GlobalSwitch_RegisterDisabled_Message, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
        }

        #endregion


        #region CanAddAddress
        
        [Test]
        public async Task CanAddAddress_CheckGeoipCountryMatchesOnly_UseOfGeoipInformationDisabled()
        {
            var config = CreateConfigForCheckGeoipCountryMatchesOnly(
                globalSwitchDisableUseOfGeoipInformation: true);
            var geoipInfoService = CreateGeoipInfoService(null, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var outcome = await service.CanAddAddress(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckGeoipCountryMatchesOnly_GeoipCheckDisabled()
        {
            var config = CreateConfigForCheckGeoipCountryMatchesOnly(
                addAddressDisableGeoipCheck: true);
            var geoipInfoService = CreateGeoipInfoService(null, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var outcome = await service.CanAddAddress(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckGeoipCountryMatchesOnly_NullGeoipInfo()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanAddAddress(userId, userIpAddress, countryId);

            Assert.IsTrue(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.AreEqual(AntiAbuseResources.CannotDetermineGeoipCountryErrorMessage,
                outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckGeoipCountryMatchesOnly_CountryNotMatching()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;
            var geoipCountryId = CountryId.GR;

            var geoipInfo = new GeoipInfo
            {
                CountryCode = EnumsHelper.CountryId.ToString(geoipCountryId)
            };

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, geoipInfo);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanAddAddress(userId, userIpAddress, countryId);

            Assert.IsTrue(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.AreEqual(
                string.Format(AntiAbuseResources.GeoipCountryMismatchErrorMessage, EnumsHelper.CountryId.ToString(geoipCountryId)),
                outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanAddAddress_CheckGeoipCountryMatchesOnly_CountryMatching()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;
            var geoipCountryId = countryId;

            var geoipInfo = new GeoipInfo
            {
                CountryCode = EnumsHelper.CountryId.ToString(geoipCountryId)
            };

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, geoipInfo);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanAddAddress(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason,
                "The RejectionReason field on the outcome is not the expected.");
        }

        #endregion

        #region CanCreateTenancyDetailsSubmission

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_CheckGeoipCountryMatchesOnly_UseOfGeoipInformationDisabled()
        {
            var config = CreateConfigForCheckGeoipCountryMatchesOnly(
                globalSwitchDisableUseOfGeoipInformation: true);
            var geoipInfoService = CreateGeoipInfoService(null, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var outcome = await service.CanCreateTenancyDetailsSubmission(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_CheckGeoipCountryMatchesOnly_GeoipCheckDisabled()
        {
            var config = CreateConfigForCheckGeoipCountryMatchesOnly(
                createTenancyDetailsSubmissionDisableGeoipCheck: true);
            var geoipInfoService = CreateGeoipInfoService(null, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var outcome = await service.CanCreateTenancyDetailsSubmission(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_CheckGeoipCountryMatchesOnly_NullGeoipInfo()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanCreateTenancyDetailsSubmission(userId, userIpAddress, countryId);

            Assert.IsTrue(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.AreEqual(AntiAbuseResources.CannotDetermineGeoipCountryErrorMessage,
                outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_CheckGeoipCountryMatchesOnly_CountryNotMatching()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;
            var geoipCountryId = CountryId.GR;

            var geoipInfo = new GeoipInfo
            {
                CountryCode = EnumsHelper.CountryId.ToString(geoipCountryId)
            };

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, geoipInfo);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanCreateTenancyDetailsSubmission(userId, userIpAddress, countryId);

            Assert.IsTrue(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.AreEqual(
                string.Format(AntiAbuseResources.GeoipCountryMismatchErrorMessage, EnumsHelper.CountryId.ToString(geoipCountryId)),
                outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanCreateTenancyDetailsSubmission_CheckGeoipCountryMatchesOnly_CountryMatching()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;
            var geoipCountryId = countryId;

            var geoipInfo = new GeoipInfo
            {
                CountryCode = EnumsHelper.CountryId.ToString(geoipCountryId)
            };

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, geoipInfo);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanCreateTenancyDetailsSubmission(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason,
                "The RejectionReason field on the outcome is not the expected.");
        }

        #endregion

        #region CanPickOutgoingVerification

        [Test]
        public async Task CanPickOutgoingVerification_CheckGeoipCountryMatchesOnly_UseOfGeoipInformationDisabled()
        {
            var config = CreateConfigForCheckGeoipCountryMatchesOnly(
                globalSwitchDisableUseOfGeoipInformation: true);
            var geoipInfoService = CreateGeoipInfoService(null, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var outcome = await service.CanPickOutgoingVerification(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckGeoipCountryMatchesOnly_GeoipCheckDisabled()
        {
            var config = CreateConfigForCheckGeoipCountryMatchesOnly(
                pickOutgoingVerificationDisableGeoipCheck: true);
            var geoipInfoService = CreateGeoipInfoService(null, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var outcome = await service.CanPickOutgoingVerification(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckGeoipCountryMatchesOnly_NullGeoipInfo()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, null);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanPickOutgoingVerification(userId, userIpAddress, countryId);

            Assert.IsTrue(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.AreEqual(AntiAbuseResources.CannotDetermineGeoipCountryErrorMessage,
                outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckGeoipCountryMatchesOnly_CountryNotMatching()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;
            var geoipCountryId = CountryId.GR;

            var geoipInfo = new GeoipInfo
            {
                CountryCode = EnumsHelper.CountryId.ToString(geoipCountryId)
            };

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, geoipInfo);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanPickOutgoingVerification(userId, userIpAddress, countryId);

            Assert.IsTrue(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.AreEqual(
                string.Format(AntiAbuseResources.GeoipCountryMismatchErrorMessage, EnumsHelper.CountryId.ToString(geoipCountryId)),
                outcome.RejectionReason, "The RejectionReason field on the outcome is not the expected.");
        }

        [Test]
        public async Task CanPickOutgoingVerification_CheckGeoipCountryMatchesOnly_CountryMatching()
        {
            var userId = "user-id";
            var userIpAddress = "1.2.3.4";
            var countryId = CountryId.GB;
            var geoipCountryId = countryId;

            var geoipInfo = new GeoipInfo
            {
                CountryCode = EnumsHelper.CountryId.ToString(geoipCountryId)
            };

            var config = CreateConfigForCheckGeoipCountryMatchesOnly();
            var geoipInfoService = CreateGeoipInfoService(userIpAddress, geoipInfo);
            var service = new AntiAbuseService(null, config, null, null, null, geoipInfoService);

            var outcome = await service.CanPickOutgoingVerification(userId, userIpAddress, countryId);

            Assert.IsFalse(outcome.IsRejected, "The IsRejected field on the outcome is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, 
                "The RejectionReason field on the outcome is not the expected.");
        }

        #endregion

        #region Private Helpers

        private IGeoipInfoService CreateGeoipInfoService(string ipAddress, GeoipInfo geoipInfoToReturn)
        {
            var mockGeoipInfoService = new Mock<IGeoipInfoService>();

            mockGeoipInfoService.Setup(x => x.GetInfoAsync(It.IsAny<string>()))
                .Returns((string ip) =>
                {
                    if (ip.Equals(ipAddress))
                        return Task.FromResult(geoipInfoToReturn);
                    else
                        throw new Exception("The expected ipAddress was not used.");
                });

            return mockGeoipInfoService.Object;
        }

        private IAntiAbuseServiceConfig CreateConfigForCheckGeoipCountryMatchesOnly(
            bool globalSwitchDisableUseOfGeoipInformation = false,
            bool addAddressDisableGeoipCheck = false,
            bool createTenancyDetailsSubmissionDisableGeoipCheck = false,
            bool pickOutgoingVerificationDisableGeoipCheck = false)
        {
            var mockAntiAbuseServiceConfig = new Mock<IAntiAbuseServiceConfig>();

            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGlobalFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableIpAddressFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableUserFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGeocodeFailureUserFrequencyCheck).Returns(true);


            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_DisableUserFrequencyCheck).Returns(true);

            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_DisableGlobalFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_DisableIpAddressFrequencyCheck).Returns(true);
            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_DisableMaxOutstandingFrequencyPerUserCheck).Returns(true);

            mockAntiAbuseServiceConfig.Setup(x => x.GlobalSwitch_DisableUseOfGeoipInformation)
                .Returns(globalSwitchDisableUseOfGeoipInformation);
            mockAntiAbuseServiceConfig.Setup(x => x.AddAddress_DisableGeoipCheck)
                .Returns(addAddressDisableGeoipCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.CreateTenancyDetailsSubmission_DisableGeoipCheck)
                .Returns(createTenancyDetailsSubmissionDisableGeoipCheck);
            mockAntiAbuseServiceConfig.Setup(x => x.PickOutgoingVerification_DisableGeoipCheck)
                .Returns(pickOutgoingVerificationDisableGeoipCheck);

            return mockAntiAbuseServiceConfig.Object;
        }

        #endregion
    }
}
