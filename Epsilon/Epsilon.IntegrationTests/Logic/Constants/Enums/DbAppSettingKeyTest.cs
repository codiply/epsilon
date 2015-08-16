using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Constants.Enums
{
    public class DbAppSettingKeyTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task ThereShouldBeAKey_ForAllDbAppSettingsInTheDatabase()
        {
            var enumKeys = EnumsHelper.DbAppSettingKey.GetNames().ToDictionary(x => x);

            var allAppSettingsInDb = await DbProbe.AppSettings.ToListAsync();

            var failingAppSettings = allAppSettingsInDb
                .Where(x => !enumKeys.ContainsKey(x.Id))
                .ToList();

            var message = "";
            if (failingAppSettings.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingAppSettings.Count() == 1 ? " is " : " are ")
                    .Append(failingAppSettings.Count())
                    .Append(" DbAppSettingKey")
                    .Append(failingAppSettings.Count() == 1 ? "" : "s")
                    .Append(" with missing key in Constants.Enums.DbAppSettingKey enumeration.");
                foreach (var setting in failingAppSettings)
                {
                    sb.Append("\n").Append(setting.Id);
                }

                message = sb.ToString();
            }

            Assert.IsFalse(failingAppSettings.Any(), message);
        }

        [Test]
        public async Task EveryDbAppSettingKeyShouldHaveAnAppSettingInTheDatabase()
        {
            var enumKeys = EnumsHelper.DbAppSettingKey.GetNames();

            var allAppSettingsInDb = 
                await DbProbe.AppSettings.ToDictionaryAsync(x => x.Id);

            var failingAppSettings = enumKeys
                .Where(x => !allAppSettingsInDb.ContainsKey(x))
                .ToList();

            var message = "";
            if (failingAppSettings.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingAppSettings.Count() == 1 ? " is " : " are ")
                    .Append(failingAppSettings.Count())
                    .Append(" DbAppSettingKey")
                    .Append(failingAppSettings.Count() == 1 ? "" : "'s")
                    .Append(" in Constants.Enums.DbAppSettingKey enumeration with missing AppSetting in the database: ")
                    .Append(String.Join(", ", failingAppSettings))
                    .Append(".");
                message = sb.ToString();
            }

            Assert.IsFalse(failingAppSettings.Any(), message);
        }
    }
}
