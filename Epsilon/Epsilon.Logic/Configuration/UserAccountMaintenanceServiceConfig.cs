﻿using Epsilon.Logic.Configuration.Interfaces;
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
    public class UserAccountMaintenanceServiceConfig : IUserAccountMaintenanceServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public UserAccountMaintenanceServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public TimeSpan OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriod
        {
            get
            {
                var periodInDays = _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriodInDays,
                    _dbAppSettingDefaultValue.OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriodInDays);
                return TimeSpan.FromDays(periodInDays);
            }
        }
    }
}
