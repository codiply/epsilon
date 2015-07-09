using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Ninject;
using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Services.Interfaces;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class UserPreferenceServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task CreateUserPreference_AndThen_GetUserPreference_AreConsistent()
        {
            var containerForCreate = CreateContainer();
            var containerForGet = CreateContainer();

            var ipAddress = "1.2.3.5";
            var languageId = "en";

            var user = await CreateUser(containerForCreate, "test@test.com", ipAddress, false);
            var serviceForCreate = containerForCreate.Get<IUserPreferenceService>();
            await serviceForCreate.Create(user.Id, languageId);

            var serviceForGet = containerForGet.Get<IUserPreferenceService>();
            var retrievedUserPreference = await serviceForGet.Get(user.Id);

            Assert.IsNotNull(retrievedUserPreference, "The UserPreference was not found.");
            Assert.AreEqual(languageId, retrievedUserPreference.LanguageId,
                "The LanguageId on UserPreference is not the expected.");
        }
    }
}
