using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration
{
    public class AntiAbuseServiceConfig : IAntiAbuseServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public AntiAbuseServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public bool Register_DisableGlobalFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_Register_DisableGlobalFrequencyCheck) == true;
            }
        }

        public Frequency Register_GlobalMaxFrequency
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_Register_GlobalMaxFrequency,
                    _dbAppSettingDefaultValue.AntiAbuse_Register_GlobalMaxFrequency);
            }
        }

        public bool Register_DisableIpAddressFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_Register_DisableIpAddressFrequencyCheck) == true;
            }
        }

        public Frequency Register_MaxFrequencyPerIpAddress
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_Register_MaxFrequencyPerIpAddress,
                    _dbAppSettingDefaultValue.AntiAbuse_Register_MaxFrequencyPerIpAddress);
            }
        }

        public bool AddAddress_DisableIpAddressFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck) == true;
            }
        }

        public Frequency AddAddress_MaxFrequencyPerIpAddress
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress,
                    _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress);
            }
        }

        public bool AddAddress_DisableUserFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_AddAddress_DisableUserFrequencyCheck) == true;
            }
        }

        public Frequency AddAddress_MaxFrequencyPerUser
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerUser,
                    _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxFrequencyPerUser);
            }
        }

        public bool CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck) == true;
            }
        }

        public Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress,
                    _dbAppSettingDefaultValue.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress);
            }
        }

        public bool CreateTenancyDetailsSubmission_DisableUserFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck) == true;
            }
        }

        public Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerUser
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser,
                    _dbAppSettingDefaultValue.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser);
            }
        }

        public Frequency GeocodeFailure_MaxFrequencyPerIpAddress
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_GeocodeFailure_MaxFrequencyPerIpAddress,
                    _dbAppSettingDefaultValue.AntiAbuse_GeocodeFailure_MaxFrequencyPerIpAddress);
            }
        }

        public bool GeocodeFailure_DisableIpAddressFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_GeocodeFailure_DisableIpAddressFrequencyCheck) == true;
            }
        }

        public Frequency GeocodeFailure_MaxFrequencyPerUser
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_GeocodeFailure_MaxFrequencyPerUser,
                    _dbAppSettingDefaultValue.AntiAbuse_GeocodeFailure_MaxFrequencyPerUser);
            }
        }

        public bool GeocodeFailure_DisableUserFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_GeocodeFailure_DisableUserFrequencyCheck) == true;
            }
        }
    }
}
