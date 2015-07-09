using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.Data.Entity;
using Epsilon.Logic.Services.Interfaces;
using NUnit.Framework;
using Epsilon.Logic.Constants;

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
                "The number of available countries was not the expected");
            foreach (var country in availableCountries)
            {
                var expectedCountry = expectedAvailableCountries[country.Id];
                Assert.IsNotNull(expectedCountry,
                    String.Format("Country with Id '{0}' is not actually an available country.", country.Id));
                Assert.AreEqual(expectedCountry.EnglishName, country.EnglishName,
                    String.Format("Field EnglishName is not the expected for Country with Id '{0}'.", country.Id));
                Assert.AreEqual(expectedCountry.LocalName, country.LocalName,
                    String.Format("Field LocalName is not the expected for Country with Id '{0}'.", country.Id));
                Assert.AreEqual(expectedCountry.CurrencyId, country.CurrencyId,
                    String.Format("Field CurrencyId is not the expected for Country with Id '{0}'.", country.Id));
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
                    String.Format("The DisplayName was not the expected for country with Id '{0}'", country.Id));
            }
        }
    }
}
