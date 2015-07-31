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
    public class GeocodeServiceConfig : IGeocodeServiceConfig
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public GeocodeServiceConfig(
            IAppSettingsHelper appSettingsHelper,
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _appSettingsHelper = appSettingsHelper;
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public string GoogleApiServerKey
        {
            get
            {
                return _appSettingsHelper.GetString(AppSettingsKey.GoogleApiServerKey);
            }
        }

        public int OverQueryLimitMaxRetries
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.GeocodeService_OverQueryLimitMaxRetries,
                    _dbAppSettingDefaultValue.GeocodeService_OverQueryLimitMaxRetries);
            }
        }

        public TimeSpan OverQueryLimitDelayBetweenRetries
        {
            get
            {
                var delayInSeconds = _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds,
                    _dbAppSettingDefaultValue.GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds);
                return TimeSpan.FromSeconds(delayInSeconds);
            }
        }
    }
}
