using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int MaxRetries
        {
            get
            {
                return  _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.GeoipInfoService_MaxRetries,
                    _dbAppSettingDefaultValue.GeoipInfoService_MaxRetries);
            }
        }
    }
}
