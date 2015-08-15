using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using System;

namespace Epsilon.Logic.Configuration
{
    public class GeoipInfoServiceConfig : IGeoipInfoServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public GeoipInfoServiceConfig(
           IDbAppSettingsHelper dbAppSettingsHelper,
           IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public TimeSpan ExpiryPeriod
        {
            get
            {
                var periodInDays = _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.GeoipInfo_ExpiryPeriodInDays,
                    _dbAppSettingDefaultValue.GeoipInfo_ExpiryPeriodInDays);
                return TimeSpan.FromDays(periodInDays);
            }
        }
    }
}
