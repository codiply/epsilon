﻿using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Constants.Enums
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
                    sb.Append(string.Format("\n{0} with value type '{1}'", x.Id, x.ValueType));
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
                // EnumSwitch:DbAppSettingValueType
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
                    case DbAppSettingValueType.String:
                        canParseValue = true;
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Unexpected DbAppSettingValueType: '{0}'",
                        EnumsHelper.DbAppSettingValueType.ToString(valueType)));
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
                    sb.Append(string.Format("\n{0} with value type '{1}' and value '{2}'", 
                        x.Id, x.ValueType, x.Value));
                }
                message = sb.ToString();
            }

            Assert.IsFalse(failingDbAppSettings.Any(), message);
        }
    }
}
