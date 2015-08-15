using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using NUnit.Framework;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class CurrencyServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetAvailableCountriesTest()
        {
            var container = CreateContainer();
            var service = container.Get<ICurrencyService>();

            var allCurrencies = service.GetAll();

            var expectedCurrencies = await
                DbProbe.Currencies.ToDictionaryAsync(x => x.Id);

            Assert.AreEqual(expectedCurrencies.Count, allCurrencies.Count,
                "The number of available countries was not the expected.");
            foreach (var currency in allCurrencies)
            {
                var expectedCurrency = expectedCurrencies[currency.Id];
                Assert.IsNotNull(expectedCurrency,
                    string.Format("Currency with Id '{0}' was not found.", currency.Id));
                Assert.AreEqual(expectedCurrency.EnglishName, currency.EnglishName,
                    string.Format("Field EnglishName is not the expected for Currency with Id '{0}'.", currency.Id));
                Assert.AreEqual(expectedCurrency.LocalName, currency.LocalName,
                    string.Format("Field LocalName is not the expected for Currency with Id '{0}'.", currency.Id));
                Assert.AreEqual(expectedCurrency.Symbol, currency.Symbol,
                    string.Format("Field Symbol is not the expected for Currency with Id '{0}'.", currency.Id));
            }
        }

        [Test]
        public async Task GetDisplayNameTest()
        {
            var container = CreateContainer();
            var service = container.Get<ICurrencyService>();

            var allCurrencies = await DbProbe.Currencies.ToListAsync();

            foreach (var currency in allCurrencies)
            {
                var expectedDisplayName = AppConstant.CURRENCY_DISPLAY_FIELD_SELECTOR(currency);
                var actualDisplayName = service.GetDisplayName(currency.Id);
                Assert.AreEqual(expectedDisplayName, actualDisplayName,
                    string.Format("The DisplayName was not the expected for currency with Id '{0}'.", currency.Id));
            }
        }

        [Test]
        public async Task GetTest()
        {
            var container = CreateContainer();
            var service = container.Get<ICurrencyService>();

            var allCurrencies = await DbProbe.Currencies.ToListAsync();

            foreach (var currency in allCurrencies)
            {
                var actualCurrency = service.Get(currency.Id);
                Assert.AreEqual(currency.EnglishName, actualCurrency.EnglishName,
                    string.Format("The EnglishName was not the expected for currency with Id '{0}'.", currency.Id));
                Assert.AreEqual(currency.LocalName, actualCurrency.LocalName,
                    string.Format("The LocalName was not the expected for currency with Id '{0}'.", currency.Id));
                Assert.AreEqual(currency.Symbol, actualCurrency.Symbol,
                    string.Format("The Symbol was not the expected for currency with Id '{0}'.", currency.Id));
            }
        }
    }
}
