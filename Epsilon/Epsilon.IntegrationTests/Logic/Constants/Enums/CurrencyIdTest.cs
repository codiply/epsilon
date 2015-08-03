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

namespace Epsilon.IntegrationTests.Logic.Constants.Enums
{
    public class CurrencyIdTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task ThereShouldBeACurrencyId_ForAllCurrenciesInTheDatabase()
        {
            var enumCurrencyIds = EnumsHelper.CurrencyId.GetNames().ToDictionary(x => x);

            var allCurrenciesInDb = await DbProbe.Currencies.ToListAsync();

            var failingCurrencies = allCurrenciesInDb
                .Where(c => !enumCurrencyIds.ContainsKey(c.Id))
                .ToList();

            var message = "";
            if (failingCurrencies.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingCurrencies.Count() == 1 ? " is " : " are ")
                    .Append(failingCurrencies.Count())
                    .Append(failingCurrencies.Count() == 1 ? " Currency" : " Currencies")
                    .Append(" with missing Id in Constants.Enums.CurrencyId enumeration.");
                foreach (var c in failingCurrencies)
                {
                    sb.Append("\n").Append(c.Id).Append(" - ").Append(c.EnglishName);
                }

                message = sb.ToString();
            }

            Assert.IsFalse(failingCurrencies.Any(), message);
        }

        [Test]
        public async Task EveryCurrencyIdShouldHaveAnCurrencyInTheDatabase()
        {
            var enumCurrencyIds = EnumsHelper.CurrencyId.GetNames();

            var allCurrenciesInDb = 
                await DbProbe.Currencies.ToDictionaryAsync(x => x.Id);

            var failingCurrencyIds = enumCurrencyIds
                .Where(id => !allCurrenciesInDb.ContainsKey(id))
                .ToList();

            var message = "";
            if (failingCurrencyIds.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingCurrencyIds.Count() == 1 ? " is " : " are ")
                    .Append(failingCurrencyIds.Count())
                    .Append(" CurrencyId")
                    .Append(failingCurrencyIds.Count() == 1 ? "" : "'s")
                    .Append(" in Constants.Enums.CurrencyId enumeration with missing Currency in the database: ")
                    .Append(String.Join(", ", failingCurrencyIds))
                    .Append(".");
                message = sb.ToString();
            }

            Assert.IsFalse(failingCurrencyIds.Any(), message);
        }
    }
}
