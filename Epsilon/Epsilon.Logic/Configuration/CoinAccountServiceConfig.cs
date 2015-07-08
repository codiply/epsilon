﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration
{
    public class CoinAccountServiceConfig : ICoinAccountServiceConfig
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IAppSettingsDefaultValue _appSettingsDefaultValue;

        public CoinAccountServiceConfig(
            IAppSettingsHelper appSettingsHelper,
            IAppSettingsDefaultValue appSettingsDefaultValue)
        {
            _appSettingsHelper = appSettingsHelper;
            _appSettingsDefaultValue = appSettingsDefaultValue;
        }

        public TimeSpan SnapshotSnoozePeriod
        {
            get
            {
                return TimeSpan.FromHours(_appSettingsHelper.GetDouble(
                    AppSettingsKey.CoinAccountSnapshotSnoozePeriodInHours,
                    _appSettingsDefaultValue.CoinAccountSnapshotSnoozePeriodInHours));
            }
        }

        public int SnapshotNumberOfTransactionsThreshold
        {
            get
            {
                return  _appSettingsHelper.GetInt(
                    AppSettingsKey.CoinAccountSnapshotNumberOfTransactionsThreshold,
                    _appSettingsDefaultValue.CoinAccountSnapshotNumberOfTransactionsThreshold);
            }
        }
    }
}