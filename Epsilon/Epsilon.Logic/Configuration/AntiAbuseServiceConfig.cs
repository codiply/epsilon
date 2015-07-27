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

        #region Add Address

        public bool AddAddress_DisableGlobalFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_AddAddress_DisableGlobalFrequencyCheck) == true;
            }
        }

        public Frequency AddAddress_GlobalMaxFrequency
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_AddAddress_GlobalMaxFrequency,
                    _dbAppSettingDefaultValue.AntiAbuse_AddAddress_GlobalMaxFrequency);
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

        public bool AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck) == true;
            }
        }

        public Frequency AddAddress_MaxGeocodeFailureFrequencyPerIpAddress
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress,
                    _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress);
            }
        }

        public bool AddAddress_DisableGeocodeFailureUserFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck) == true;
            }
        }

        public Frequency AddAddress_MaxGeocodeFailureFrequencyPerUser
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser,
                    _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser);
            }
        }

        #endregion

        #region Create Tenancy Details Submission

        public bool CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck) == true;
            }
        }

        public Frequency CreateTenancyDetailsSubmission_GlobalMaxFrequency
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency,
                    _dbAppSettingDefaultValue.AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency);
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

        #endregion

        #region Pick Outgoing Verification

        public bool PickOutgoingVerification_DisableGlobalFrequencyCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck) == true;
            }
        }

        public Frequency PickOutgoingVerification_GlobalMaxFrequency
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency,
                    _dbAppSettingDefaultValue.AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency);
            }
        }

        #endregion


        #region Register

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

        #endregion
    }
}
