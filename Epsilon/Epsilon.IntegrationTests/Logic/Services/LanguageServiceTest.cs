using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Ninject;
using Epsilon.Logic.Services.Interfaces;
using NUnit.Framework;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class LanguageServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetAvailableLanguagesTest()
        {
            var container = CreateContainer();
            var service = container.Get<ILanguageService>();

            var languages = service.GetAvailableLanguages();

            var actualLanguages = await 
                DbProbe.Languages.Where(x => x.IsAvailable).ToDictionaryAsync(x => x.Id);

            Assert.AreEqual(actualLanguages.Count, languages.Count,
                "The number of available languages was not the expected");
            foreach (var lang in languages)
            {
                var actualLanguage = actualLanguages[lang.Id];
                Assert.IsNotNull(actualLanguage,
                    String.Format("Language with id {0} is not actually an available language.", lang.Id));
                Assert.AreEqual(actualLanguage.EnglishName, lang.EnglishName, 
                    String.Format("Field EnglishName is not the expected for Language with Id {0}.", lang.Id));
                Assert.AreEqual(actualLanguage.LocalName, lang.LocalName,
                    String.Format("Field LocalName is not the expected", lang.Id));
                Assert.AreEqual(actualLanguage.CultureCode, lang.CultureCode, 
                    String.Format("Field CultureCode is not the expected for Language with Id {0}.", lang.Id));
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
                    String.Format("Field Language with id {0} could not be retrieved.", lang.Id));
                Assert.AreEqual(lang.EnglishName, retrievedLanguage.EnglishName,
                    String.Format("Field EnglishName is not the expected for Language with Id {0}.", lang.Id));
                Assert.AreEqual(lang.LocalName, retrievedLanguage.LocalName,
                    String.Format("Field LocalName is not the expected", lang.Id));
                Assert.AreEqual(lang.CultureCode, retrievedLanguage.CultureCode,
                    String.Format("Field CultureCode is not the expected for Language with Id {0}.", lang.Id));
                Assert.AreEqual(lang.IsAvailable, retrievedLanguage.IsAvailable,
                    String.Format("Field IsAvailable is not the expected for Language with Id {0}.", lang.Id));
            }
        }
    }
}
