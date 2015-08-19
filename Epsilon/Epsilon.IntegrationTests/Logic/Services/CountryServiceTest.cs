using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using NUnit.Framework;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class CountryServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetAvailableCountriesTest()
        {
            var container = CreateContainer();
            var service = container.Get<ICountryService>();

            var availableCountries = service.GetAvailableCountries();

            var expectedAvailableCountries = await
                DbProbe.Countries.Where(x => x.IsAvailable).ToDictionaryAsync(x => x.Id);

            Assert.AreEqual(expectedAvailableCountries.Count, availableCountries.Count,
                "The number of available countries was not the expected.");
            foreach (var country in availableCountries)
            {
                var expectedCountry = expectedAvailableCountries[country.Id];
                Assert.IsNotNull(expectedCountry,
                    string.Format("Country with Id '{0}' is not actually an available country.", country.Id));
                Assert.AreEqual(expectedCountry.EnglishName, country.EnglishName,
                    string.Format("Field EnglishName is not the expected for Country with Id '{0}'.", country.Id));
                Assert.AreEqual(expectedCountry.LocalName, country.LocalName,
                    string.Format("Field LocalName is not the expected for Country with Id '{0}'.", country.Id));
                Assert.AreEqual(expectedCountry.CurrencyId, country.CurrencyId,
                    string.Format("Field CurrencyId is not the expected for Country with Id '{0}'.", country.Id));

                var isCountryAvailable = service.IsCountryAvailable(country.IdAsEnum.Value);
                Assert.IsTrue(isCountryAvailable, "service.IsCountryAvailable did not return the expected result.");
            }
        }

        [Test]
        public async Task GetCountryTest()
        {
            var container = CreateContainer();
            var service = container.Get<ICountryService>();

            var allCountriesInDb = await
                DbProbe.Countries.ToDictionaryAsync(x => x.Id);

            foreach (var countryId in allCountriesInDb.Keys)
            {
                var expectedCountry = allCountriesInDb[countryId];
                var country = service.GetCountry(expectedCountry.Id);
                Assert.IsNotNull(country,
                    string.Format("Country with Id '{0}' is not found.", country.Id));
                Assert.AreEqual(expectedCountry.EnglishName, country.EnglishName,
                    string.Format("Field EnglishName is not the expected for Country with Id '{0}'.", country.Id));
                Assert.AreEqual(expectedCountry.LocalName, country.LocalName,
                    string.Format("Field LocalName is not the expected for Country with Id '{0}'.", country.Id));
                Assert.AreEqual(expectedCountry.CurrencyId, country.CurrencyId,
                    string.Format("Field CurrencyId is not the expected for Country with Id '{0}'.", country.Id));
                Assert.AreEqual(expectedCountry.IsAvailable, country.IsAvailable,
                    string.Format("Field IsAvailable is not the expected for Country with Id '{0}'.", country.Id));
            }
        }

        [Test]
        public async Task GetDisplayNameTest()
        {
            var container = CreateContainer();
            var service = container.Get<ICountryService>();

            var allCountries = await DbProbe.Countries.ToListAsync();

            foreach (var country in allCountries)
            {
                var expectedDisplayName = AppConstant.COUNTRY_DISPLAY_FIELD_SELECTOR(country);
                var actualDisplayName = service.GetDisplayName(country.Id);
                Assert.AreEqual(expectedDisplayName, actualDisplayName,
                    string.Format("The DisplayName was not the expected for country with Id '{0}'.", country.Id));
            }
        }
    }
}
