using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using NUnit.Framework;
using System.Data.Entity;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Helpers;

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
