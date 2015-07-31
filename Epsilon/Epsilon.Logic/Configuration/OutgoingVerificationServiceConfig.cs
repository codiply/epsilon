using Epsilon.Logic.Configuration.Interfaces;
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
    public class OutgoingVerificationServiceConfig : IOutgoingVerificationServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public OutgoingVerificationServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public bool GlobalSwitch_DisablePickOutgoingVerification
        {
            get
            {
                return _dbAppSettingsHelper.GetBool(DbAppSettingKey.GlobalSwitch_DisablePickOutgoingVerification) == true;
            }
        }

        public int MyOutgoingVerificationsSummary_ItemsLimit
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit,
                    _dbAppSettingDefaultValue.OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit);
            }
        }

        public int VerificationsPerTenancyDetailsSubmission
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.OutgoingVerification_VerificationsPerTenancyDetailsSubmission,
                    _dbAppSettingDefaultValue.OutgoingVerification_VerificationsPerTenancyDetailsSubmission);
            }
        }
    }
}
