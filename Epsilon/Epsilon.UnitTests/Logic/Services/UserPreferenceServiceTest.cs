using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Manage;
using Epsilon.Logic.Services;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class UserPreferenceServiceTest
    {
        [Test]
        public async Task ChangePreferences_WithInvalidLanguage_Fails()
        {
            var userId = "some-user-id";
            var languageId = "invalid";
            var languageService = CreateLanguageService(languageId, null);
            var userPreferenceService = new UserPreferenceService(null, null, null, languageService);

            var form = new ChangePreferencesForm { LanguageId = languageId };
            var outcome = await userPreferenceService.ChangePreferences(userId, form);

            Assert.IsFalse(outcome.IsSuccess, "IsSuccess on outcome is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, outcome.ErrorMessage, 
                "ErrorMessage on outcome is not the expected.");
            Assert.IsFalse(outcome.ReturnToForm, "ReturnToForm on outcome is not the expected.");
        }

        [Test]
        public async Task ChangePreferences_WithUnavailableLanguage_Fails()
        {
            var userId = "some-user-id";
            var languageId = "invalid";
            var languageIsAvailable = false;

            var language = new Language { IsAvailable = languageIsAvailable };
            var languageService = CreateLanguageService(languageId, language);
            var userPreferenceService = new UserPreferenceService(null, null, null, languageService);

            var form = new ChangePreferencesForm { LanguageId = languageId };
            var outcome = await userPreferenceService.ChangePreferences(userId, form);

            Assert.IsFalse(outcome.IsSuccess, "IsSuccess on outcome is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, outcome.ErrorMessage,
                "ErrorMessage on outcome is not the expected.");
            Assert.IsFalse(outcome.ReturnToForm, "ReturnToForm on outcome is not the expected.");
        }

        private ILanguageService CreateLanguageService(string languageId, Language languageToReturn)
        {
            var mockLanguageService = new Mock<ILanguageService>();

            mockLanguageService.Setup(x => x.GetLanguage(It.IsAny<string>()))
                .Returns((string langId) =>
                {
                    if (langId.Equals(languageId))
                        return languageToReturn;
                    else
                        throw new Exception(
                            string.Format("LanguageService.GetLanguage was passed in '{0}' instead of '{1}'.", langId, languageId));
                });

            return mockLanguageService.Object;
        }
    }
}
