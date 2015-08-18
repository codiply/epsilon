using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Forms.Admin;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Ninject;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Helpers
{
    public class DbAppSettingsHelperTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task ValuesAreFetchedFromTheDatabaseAndCached()
        {
            var allExpectedSettings = await DbProbe.AppSettings.ToListAsync();

            var container = CreateContainer();
            var dbContext = container.Get<IEpsilonContext>();
            var helper = container.Get<IDbAppSettingsHelper>();

            foreach (var expectedSetting in allExpectedSettings)
            {
                var id = expectedSetting.Id;
                var originalValue = expectedSetting.Value;
                var actualSettingValue = helper.GetString(id);
                Assert.AreEqual(originalValue, actualSettingValue,
                    string.Format("The value for DbAppSetting with Id '{0}' was not the expected.", id));

                // I update the value
                var newValue = "new-value";
                expectedSetting.Value = newValue;
                dbContext.AppSettings.Attach(expectedSetting);
                dbContext.Entry(expectedSetting).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();

                // I check the value has been updated in the database.
                var fetchedEntity = await DbProbe.AppSettings.FindAsync(expectedSetting.Id);
                Assert.AreEqual(newValue, fetchedEntity.Value,
                    string.Format("The value for DbAppSetting with Id '{0}' was not updated in the database.", id));

                // I get the value again from the helper and expect it to be the originalValue
                var actualSettingValueAfterUpdate = helper.GetString(id);
                Assert.AreEqual(originalValue, actualSettingValueAfterUpdate,
                    string.Format("The value for DbAppSetting with Id '{0}' was not the cached.", id));
            }
        }

        [Test]
        public async Task UpdateRefreshesTheCache()
        {
            var container = CreateContainer();
            var helper = container.Get<IDbAppSettingsHelper>();


            var user = await CreateUser(container, "test@test.com", "1.2.3.4");

            var keyEnum = DbAppSettingKey.Address_SearchAddressResultsLimit;
            var key = EnumsHelper.DbAppSettingKey.ToString(keyEnum);

            var originalValue = helper.GetInt(keyEnum);

            Assert.IsNotNull(originalValue, "OriginalValue must not be null.");

            var newValue = originalValue.Value + 1;
            var form = new DbAppSettingForm
            {
                Id = key,
                Value = newValue.ToString()
            };

            await helper.Update(form, user.Id);

            var retrievedNewValueViaHelper = helper.GetInt(keyEnum);

            Assert.AreEqual(newValue, retrievedNewValueViaHelper,
                "The retrieved new value via the helper is not the expected.");

            var retrievedNewValueViaDbProbe = 
                int.Parse((await DbProbe.AppSettings.SingleOrDefaultAsync(x => x.Id.Equals(key))).Value);

            Assert.AreEqual(newValue, retrievedNewValueViaDbProbe,
                "The retrieved new value via the DbProbe is not the expected.");
        }

        [Test]
        public async Task AllValuesInDbAppSettingKeyEnumHaveACorrespondingEntryInTheDatabase()
        {
            var allSettings = await DbProbe.AppSettings.ToDictionaryAsync(x => x.Id);

            var allEnumNames = EnumsHelper.DbAppSettingKey.GetNames();

            foreach (var key in allEnumNames)
            {
                Assert.IsTrue(allSettings.ContainsKey(key),
                    String.Format("DbAppSettingKey '{0}' was not found in the database.", key));
            }
        }

        [Test]
        public async Task AllAppSettingsInTheDatabaseHaveACorrespondingValueInDbAppSettingKeyEnum()
        {
            var allSettings = await DbProbe.AppSettings.ToDictionaryAsync(x => x.Id);

            foreach (var setting in allSettings)
            {
                var key = setting.Key;
                var enumOption = EnumsHelper.DbAppSettingKey.Parse(key);
                Assert.IsTrue(enumOption.HasValue,
                    String.Format("DbAppSettingKey '{0}' exists in the database but there is no enum option for it.", key));
            }
        }
    }
}
