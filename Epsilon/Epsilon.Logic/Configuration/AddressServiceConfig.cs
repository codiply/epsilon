﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;

namespace Epsilon.Logic.Configuration
{
    public class AddressServiceConfig : IAddressServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public AddressServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public bool GlobalSwitch_DisableAddAddress
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.GlobalSwitch_DisableAddAddress) == true;
            }
        }

        public int SearchAddressResultsLimit
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.Address_SearchAddressResultsLimit),
                    _dbAppSettingDefaultValue.Address_SearchAddressResultsLimit);
            }
        }

        public int SearchPropertyResultsLimit
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.Address_SearchPropertyResultsLimit),
                    _dbAppSettingDefaultValue.Address_SearchPropertyResultsLimit);
            }
        }
    }
}
