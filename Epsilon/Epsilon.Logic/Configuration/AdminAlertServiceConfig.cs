using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration
{
    public class AdminAlertServiceConfig : IAdminAlertServiceConfig
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public AdminAlertServiceConfig(
            IAppSettingsHelper appSettingsHelper,
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _appSettingsHelper = appSettingsHelper;
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public string ApplicationName
        {
            get
            {
                return _appSettingsHelper.GetString(AppSettingsKey.ApplicationName);
            }
        }

        public string EmailList
        {
            get
            {
               return _appSettingsHelper.GetString(AppSettingsKey.AdminAlertEmailList);
            }
        }

        public TimeSpan SnoozePeriod
        {
            get
            {
                var value = _dbAppSettingsHelper.GetDouble(
                    EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.AdminAlertSnoozePeriodInHours),
                    _dbAppSettingDefaultValue.AdminAlertSnoozePeriodInHours);
                return TimeSpan.FromHours(value);
            }
        }
    }
}
