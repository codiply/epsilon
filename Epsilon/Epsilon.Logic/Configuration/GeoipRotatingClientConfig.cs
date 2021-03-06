﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;

namespace Epsilon.Logic.Configuration
{
    public class GeoipRotatingClientConfig : IGeoipRotatingClientConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public GeoipRotatingClientConfig(
           IDbAppSettingsHelper dbAppSettingsHelper,
           IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public int MaxRotations
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.GeoipRotatingClient_MaxRotations,
                    _dbAppSettingDefaultValue.GeoipRotatingClient_MaxRotations);
            }
        }

        public string ProviderRotation
        {
            get
            {
                return _dbAppSettingsHelper.GetString(DbAppSettingKey.GeoipRotatingClient_ProviderRotation);
            }
        }
    }
}
