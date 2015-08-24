using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using NUnit.Framework;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class LanguageServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetAvailableLanguagesTest()
        {
            var container = CreateContainer();
            var service = container.Get<ILanguageService>();

            var availableLanguages = service.GetAvailableLanguages();

            var expectedAvailableLanguages = await 
                DbProbe.Languages.Where(x => x.IsAvailable).ToDictionaryAsync(x => x.Id);

            Assert.AreEqual(expectedAvailableLanguages.Count, availableLanguages.Count,
                "The number of available languages was not the expected");
            foreach (var lang in availableLanguages)
            {
                var expectedAvailableLang = expectedAvailableLanguages[lang.Id];
                Assert.IsNotNull(expectedAvailableLang,
                    string.Format("Language with id {0} is not actually an available language.", lang.Id));
                Assert.AreEqual(expectedAvailableLang.EnglishName, lang.EnglishName, 
                    string.Format("Field EnglishName is not the expected for Language with Id {0}.", lang.Id));
                Assert.AreEqual(expectedAvailableLang.LocalName, lang.LocalName,
                    string.Format("Field LocalName is not the expected for Language with Id {0}", lang.Id));
                Assert.AreEqual(expectedAvailableLang.CultureCode, lang.CultureCode, 
                    string.Format("Field CultureCode is not the expected for Language with Id {0}.", lang.Id));
            }
        }

        [Test]
        public async Task GetAvailableAndUnavailableLanguagesTest()
        {
            var container = CreateContainer();
            var service = container.Get<ILanguageService>();

            var languages = service.GetAvailableAndUnavailableLanguages();

            var expectedLanguages = await
                DbProbe.Languages.ToDictionaryAsync(x => x.Id);

            Assert.AreEqual(expectedLanguages.Count, languages.Count,
                "The number of languages was not the expected");
            foreach (var lang in languages)
            {
                var expectedLang = expectedLanguages[lang.Id];
                Assert.IsNotNull(expectedLang,
                    string.Format("Language with id {0} is not found.", lang.Id));
                Assert.AreEqual(expectedLang.EnglishName, lang.EnglishName,
                    string.Format("Field EnglishName is not the expected for Language with Id {0}.", lang.Id));
                Assert.AreEqual(expectedLang.LocalName, lang.LocalName,
                    string.Format("Field LocalName is not the expected for Language with Id {0}", lang.Id));
                Assert.AreEqual(expectedLang.CultureCode, lang.CultureCode,
                    string.Format("Field CultureCode is not the expected for Language with Id {0}.", lang.Id));
                Assert.AreEqual(expectedLang.IsAvailable, lang.IsAvailable,
                    string.Format("Field IsAvailable is not the expected for Language with Id {0}.", lang.Id));
            }
        }

        [Test]
        public async Task GetLanguageTest()
        {
            var container = CreateContainer();
            var service = container.Get<ILanguageService>();

            var allLanguages = await DbProbe.Languages.ToListAsync();

            foreach (var lang in allLanguages)
            {
                var retrievedLanguage = service.GetLanguage(lang.Id);
                Assert.IsNotNull(retrievedLanguage,
                    string.Format("Field Language with Id '{0}' could not be retrieved.", lang.Id));
                Assert.AreEqual(lang.EnglishName, retrievedLanguage.EnglishName,
                    string.Format("Field EnglishName is not the expected for Language with Id '{0}'.", lang.Id));
                Assert.AreEqual(lang.LocalName, retrievedLanguage.LocalName,
                    string.Format("Field LocalName is not the expected", lang.Id));
                Assert.AreEqual(lang.CultureCode, retrievedLanguage.CultureCode,
                    string.Format("Field CultureCode is not the expected for Language with Id '{0}'.", lang.Id));
                Assert.AreEqual(lang.IsAvailable, retrievedLanguage.IsAvailable,
                    string.Format("Field IsAvailable is not the expected for Language with Id '{0}'.", lang.Id));
            }
        }
    }
}
