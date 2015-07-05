using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using NUnit.Framework;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Entities;

namespace Epsilon.IntegrationTests.Logic.Constants
{
    public class DbAppSettingValueTypeTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task AllDbAppSetingsHaveExistingValueType()
        {
            var dbAppSettings = await DbProbe.AppSettings.ToListAsync();

            var failingDbAppSettings = dbAppSettings.Where(x =>
                !EnumsHelper.DbAppSettingValueType.Parse(x.ValueType).HasValue);

            var message = "";
            if (failingDbAppSettings.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingDbAppSettings.Count() == 1 ? " is " : " are ")
                    .Append(failingDbAppSettings.Count())
                    .Append(" DbAppSetting")
                    .Append(failingDbAppSettings.Count() == 1 ? "" : "'s")
                    .Append(" with unexpected value type: ");
                foreach (var x in failingDbAppSettings)
                {
                    sb.Append(String.Format("\n{0} with value type '{1}'", x.Id, x.ValueType));
                }
                message = sb.ToString();
            }

            Assert.IsFalse(failingDbAppSettings.Any(), message);
        }

        [Test]
        public async Task AllDbAppSettingValuesCanBeParsedAccordingToTheirType()
        {
            var parseHelper = new ParseHelper();
            var dbAppSettings = await DbProbe.AppSettings.ToListAsync();

            var failingDbAppSettings = new List<AppSetting>();
            foreach (var setting in dbAppSettings)
            {
                var valueType = EnumsHelper.DbAppSettingValueType.Parse(setting.ValueType).Value;
                bool canParseValue = false;
                switch (valueType)
                {
                    case DbAppSettingValueType.Boolean:
                        canParseValue = parseHelper.ParseBool(setting.Value).HasValue;
                        break;
                    case DbAppSettingValueType.Double:
                        canParseValue = parseHelper.ParseDouble(setting.Value).HasValue;
                        break;
                    case DbAppSettingValueType.Frequency:
                        canParseValue = parseHelper.ParseFrequency(setting.Value) != null;
                        break;
                    case DbAppSettingValueType.Integer:
                        canParseValue = parseHelper.ParseInt(setting.Value).HasValue;
                        break;
                }
                if (!canParseValue)
                    failingDbAppSettings.Add(setting);
            }

            var message = "";
            if (failingDbAppSettings.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingDbAppSettings.Count() == 1 ? " is " : " are ")
                    .Append(failingDbAppSettings.Count())
                    .Append(" DbAppSetting")
                    .Append(failingDbAppSettings.Count() == 1 ? "" : "'s")
                    .Append(" with value that cannot be parsed: ");
                foreach (var x in failingDbAppSettings)
                {
                    sb.Append(String.Format("\n{0} with value type '{1}' and value '{2}'", 
                        x.Id, x.ValueType, x.Value));
                }
                message = sb.ToString();
            }

            Assert.IsFalse(failingDbAppSettings.Any(), message);
        }
    }
}
