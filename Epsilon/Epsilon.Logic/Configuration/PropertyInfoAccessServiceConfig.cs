using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using System;

namespace Epsilon.Logic.Configuration
{
    public class PropertInfoAccessServiceConfig : IPropertyInfoAccessServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public PropertInfoAccessServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public double ExpiryPeriodInDays
        {
            get
            {
                return _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.PropertInfoAccess_ExpiryPeriodInDays,
                    _dbAppSettingDefaultValue.PropertInfoAccess_ExpiryPeriodInDays);
            }
        }

        public bool GlobalSwitch_DisableCreatePropertyInfoAccess
        {
            get
            {
                return _dbAppSettingsHelper.GetBool(DbAppSettingKey.GlobalSwitch_DisableCreatePropertyInfoAccess) == true;
            }
        }

        public TimeSpan MyExploredPropertiesSummary_CachingPeriod
        {
            get
            {
                var periodInMinutes = _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.PropertInfoAccess_MyExploredPropertiesSummary_CachingPeriodInMinutes,
                    _dbAppSettingDefaultValue.PropertInfoAccess_MyExploredPropertiesSummary_CachingPeriodInMinutes);
                return TimeSpan.FromMinutes(periodInMinutes);
            }
        }

        public int MyExploredPropertiesSummary_ItemsLimit
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.PropertInfoAccess_MyExploredPropertiesSummary_ItemsLimit,
                    _dbAppSettingDefaultValue.PropertInfoAccess_MyExploredPropertiesSummary_ItemsLimit);
            }
        }
    }
}
