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
                    EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.TenancyDetailsSubmission_Create_MaxFrequencyPerAddress),
                    _dbAppSettingDefaultValue.TenancyDetailsSubmission_Create_MaxFrequencyPerAddress);
            }
        }

        public bool Create_DisableFrequencyPerAddressCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck)) == true;
            }
        }

        public int MySubmissionsSummary_ItemsLimit
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit),
                    _dbAppSettingDefaultValue.TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit);
            }
        }
    }
}
