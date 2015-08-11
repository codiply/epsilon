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
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Entities;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class UserPreferenceServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public void CreateUser_DatabasePopulatesCreateOnField()
        {
            var container = CreateContainer();
            var dbContext = container.Get<IEpsilonContext>();
            var clock = container.Get<IClock>();

            
            var email = "test@test.com";
            var user = new User
            {
                Email = email,
                PasswordHash = "PasswordHash",
                UserName = email
            };
            dbContext.Users.Add(user);

            var timeBefore = clock.OffsetNow;
            dbContext.SaveChanges();
            var timeAfter = clock.OffsetNow;

            var retrievedUser = DbProbe.Users.Find(user.Id);

            Assert.IsTrue(timeBefore <= retrievedUser.CreatedOn && retrievedUser.CreatedOn <= timeAfter,
                "The User field CreateOn is not within the expected range.");
        }

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
            var retrievedUserPreference = await serviceForGet.GetAsync(user.Id);

            Assert.IsNotNull(retrievedUserPreference, "The UserPreference was not found.");
            Assert.AreEqual(languageId, retrievedUserPreference.LanguageId,
                "The LanguageId on UserPreference is not the expected.");
        }
    }
}
