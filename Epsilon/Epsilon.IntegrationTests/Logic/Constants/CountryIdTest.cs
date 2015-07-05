using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Helpers;

namespace Epsilon.IntegrationTests.Logic.Constants
{
    public class CountryIdTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task ThereShouldBeACountryId_ForAllAvailableCountriesInTheDatabase()
        {
            var enumCountryIds = EnumsHelper.CountryId.GetNames().ToDictionary(x => x);

            var availableCountriesInDb = await DbProbe.Countries.Where(c => c.IsAvailable).ToListAsync();

            var failingCountries = availableCountriesInDb
                .Where(c => !enumCountryIds.ContainsKey(c.Id))
                .ToList();

            var message = "";
            if (failingCountries.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingCountries.Count() == 1 ? " is " : " are ")
                    .Append(failingCountries.Count())
                    .Append(failingCountries.Count() == 1 ? " Country" : " Countries")
                    .Append(" with missing Id in Constants.CountryId enumeration.");
                foreach (var c in failingCountries)
                {
                    sb.Append("\n").Append(c.Id).Append(" - ").Append(c.EnglishName);
                }

                message = sb.ToString();
            }

            Assert.IsFalse(failingCountries.Any(), message);
        }

        [Test]
        public async Task EveryCountryIdShouldHaveAnAvailableCountryInTheDatabase()
        {
            var enumCountryIds = EnumsHelper.CountryId.GetNames();

            var availableCountriesInDb = 
                await DbProbe.Countries.Where(c => c.IsAvailable).ToDictionaryAsync(x => x.Id);

            var failingCountryIds = enumCountryIds
                .Where(id => !availableCountriesInDb.ContainsKey(id))
                .ToList();

            var message = "";
            if (failingCountryIds.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingCountryIds.Count() == 1 ? " is " : " are ")
                    .Append(failingCountryIds.Count())
                    .Append(" CountryId")
                    .Append(failingCountryIds.Count() == 1 ? "" : "'s")
                    .Append(" in Constants.CountryId enumeration with missing Country in the database: ")
                    .Append(String.Join(", ", failingCountryIds))
                    .Append(".");
                message = sb.ToString();
            }

            Assert.IsFalse(failingCountryIds.Any(), message);
        }
    }
}
