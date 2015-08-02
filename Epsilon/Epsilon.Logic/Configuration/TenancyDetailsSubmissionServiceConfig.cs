using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration
{
    public class TenancyDetailsSubmissionServiceConfig : ITenancyDetailsSubmissionServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public TenancyDetailsSubmissionServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public Frequency Create_MaxFrequencyPerAddress
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.TenancyDetailsSubmission_Create_MaxFrequencyPerAddress,
                    _dbAppSettingDefaultValue.TenancyDetailsSubmission_Create_MaxFrequencyPerAddress);
            }
        }

        public bool Create_DisableFrequencyPerAddressCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck) == true;
            }
        }

        public bool GlobalSwitch_DisableCreateTenancyDetailsSubmission
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.GlobalSwitch_DisableCreateTenancyDetailsSubmission) == true;
            }
        }

        public TimeSpan TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriod 
        { 
            get
            {
                var periodInMinutes = _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriodInMinutes,
                    _dbAppSettingDefaultValue.TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriodInMinutes);
                return TimeSpan.FromMinutes(periodInMinutes);
            } 
        }

        public int MySubmissionsSummary_ItemsLimit
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit,
                    _dbAppSettingDefaultValue.TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit);
            }
        }
    }
}
