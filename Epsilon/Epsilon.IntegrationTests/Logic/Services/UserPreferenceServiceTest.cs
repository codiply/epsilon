using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Ninject;
using NUnit.Framework;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class UserPreferenceServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public void CreateUser_DatabasePopulatesCreatedOnField()
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
        public async Task CreateUserPreference_AndThen_Get_AreConsistent()
        {
            var containerForCreate = CreateContainer();
            var containerForGet = CreateContainer();
            var clock = containerForCreate.Get<IClock>();

            var ipAddress = "1.2.3.5";
            var languageId = "en";

            var user = await CreateUser(containerForCreate, "test@test.com", ipAddress, false);
            var serviceForCreate = containerForCreate.Get<IUserPreferenceService>();
            var timeBefore = clock.OffsetNow;
            await serviceForCreate.Create(user.Id, languageId);
            var timeAfter = clock.OffsetNow;

            var serviceForGet = containerForGet.Get<IUserPreferenceService>();
            var retrievedUserPreference = serviceForGet.Get(user.Id);

            Assert.IsNotNull(retrievedUserPreference, "The UserPreference is null.");
            Assert.AreEqual(languageId, retrievedUserPreference.LanguageId,
                "The LanguageId on UserPreference is not the expected.");

            Assert.IsTrue(timeBefore <= retrievedUserPreference.UpdatedOn && retrievedUserPreference.UpdatedOn <= timeAfter,
                "The UserPreference field UpdatedOn is not within the expected range.");

            var newLanguageId = "el";

            await UpdateLanguageIdUsingDbProbe(user.Id, newLanguageId);

            var containerForGet2 = CreateContainer();
            var serviceForGet2 = containerForGet2.Get<IUserPreferenceService>();
            var retrievedUserPreferenceCached = serviceForGet2.Get(user.Id, allowCaching: true);

            Assert.IsNotNull(retrievedUserPreferenceCached, "The Cached UserPreference is null.");
            Assert.AreEqual(languageId, retrievedUserPreferenceCached.LanguageId,
                "The LanguageId on Cached UserPreference is not the expected.");

            var retrievedUserPreferenceNoncached = serviceForGet2.Get(user.Id, allowCaching: false);

            Assert.IsNotNull(retrievedUserPreferenceNoncached, "The NonCached UserPreference is null.");
            Assert.AreEqual(newLanguageId, retrievedUserPreferenceNoncached.LanguageId,
                "The LanguageId on NonCached UserPreference is not the expected.");
        }

        [Test]
        public async Task CreateUserPreference_AndThen_GetAsync_AreConsistent()
        {
            var containerForCreate = CreateContainer();
            var containerForGet = CreateContainer();
            var clock = containerForCreate.Get<IClock>();
            
            var ipAddress = "1.2.3.5";
            var languageId = "en";

            var user = await CreateUser(containerForCreate, "test@test.com", ipAddress, false);
            var serviceForCreate = containerForCreate.Get<IUserPreferenceService>();
            var timeBefore = clock.OffsetNow;
            await serviceForCreate.Create(user.Id, languageId);
            var timeAfter = clock.OffsetNow;

            var serviceForGet = containerForGet.Get<IUserPreferenceService>();
            var retrievedUserPreference = await serviceForGet.GetAsync(user.Id);

            Assert.IsNotNull(retrievedUserPreference, "The UserPreference is null.");
            Assert.AreEqual(languageId, retrievedUserPreference.LanguageId,
                "The LanguageId on UserPreference is not the expected.");

            Assert.IsTrue(timeBefore <= retrievedUserPreference.UpdatedOn && retrievedUserPreference.UpdatedOn <= timeAfter,
                "The UserPreference field UpdatedOn is not within the expected range.");

            var newLanguageId = "el";

            await UpdateLanguageIdUsingDbProbe(user.Id, newLanguageId);

            var containerForGet2 = CreateContainer();
            var serviceForGet2 = containerForGet2.Get<IUserPreferenceService>();
            var retrievedUserPreferenceCached = await serviceForGet2.GetAsync(user.Id, allowCaching: true);

            Assert.IsNotNull(retrievedUserPreferenceCached, "The Cached UserPreference is null.");
            Assert.AreEqual(languageId, retrievedUserPreferenceCached.LanguageId,
                "The LanguageId on Cached UserPreference is not the expected.");

            var retrievedUserPreferenceNoncached = await serviceForGet2.GetAsync(user.Id, allowCaching: false);

            Assert.IsNotNull(retrievedUserPreferenceNoncached, "The NonCached UserPreference is null.");
            Assert.AreEqual(newLanguageId, retrievedUserPreferenceNoncached.LanguageId,
                "The LanguageId on NonCached UserPreference is not the expected.");
        }

        
        private async Task UpdateLanguageIdUsingDbProbe(string userId, string languageId)
        {
            var userPreference = await DbProbe.UserPreferences.FindAsync(userId);

            userPreference.LanguageId = languageId;

            DbProbe.Entry(userPreference).State = EntityState.Modified;

            await DbProbe.SaveChangesAsync();
        }
    }
}
