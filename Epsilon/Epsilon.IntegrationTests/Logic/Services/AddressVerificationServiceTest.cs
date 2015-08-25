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

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AddressVerificationServiceTest : BaseIntegrationTestWithRollback
    {
        private readonly TimeSpan DelayBetweenCallsToTheApi = TimeSpan.FromSeconds(0.4);

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
        }
    }
}
