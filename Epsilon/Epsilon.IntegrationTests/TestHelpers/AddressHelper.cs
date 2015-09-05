using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Ninject;
using System;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.TestHelpers
{
    public static class AddressHelper
    {
        public static async Task<Address> CreateRandomAddressAndSave(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress, CountryId countryId, string distinctAddressCode = null)
        {
            var randomFieldLength = 10;
            var stringCountryId = EnumsHelper.CountryId.ToString(countryId);
            var postcode = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed);
            var postcodeGeometry = new PostcodeGeometry()
            {
                CountryId = stringCountryId,
                Postcode = postcode,
                Latitude = random.NextDouble(),
                Longitude = random.NextDouble(),
                ViewportNortheastLatitude = random.NextDouble(),
                ViewportNortheastLongitude = random.NextDouble(),
                ViewportSouthwestLatitude = random.NextDouble(),
                ViewportSouthwestLongitude = random.NextDouble()
            };
            var geometry = new AddressGeometry()
            {
                Latitude = random.NextDouble(),
                Longitude = random.NextDouble(),
                ViewportNortheastLatitude = random.NextDouble(),
                ViewportNortheastLongitude = random.NextDouble(),
                ViewportSouthwestLatitude = random.NextDouble(),
                ViewportSouthwestLongitude = random.NextDouble()
            };
            var address = new Address
            {
                UniqueId = Guid.NewGuid(),
                Line1 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line2 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line3 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line4 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Locality = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Region = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Postcode = postcode,
                CountryId = stringCountryId,
                CreatedById = userId,
                CreatedByIpAddress = userIpAddress,
                PostcodeGeometry = postcodeGeometry,
                Geometry = geometry,
                DistinctAddressCode = distinctAddressCode
            };
            var dbContext = container.Get<IEpsilonContext>();
            dbContext.Addresses.Add(address);
            await dbContext.SaveChangesAsync();
            return address;
        }
    }
}
