using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using System;

namespace Epsilon.Logic.Configuration
{
    public class TokenAccountServiceConfig : ITokenAccountServiceConfig
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IAppSettingsDefaultValue _appSettingsDefaultValue;

        public TokenAccountServiceConfig(
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
                    AppSettingsKey.TokenAccountSnapshotSnoozePeriodInHours,
                    _appSettingsDefaultValue.TokenAccountSnapshotSnoozePeriodInHours));
            }
        }

        public int SnapshotNumberOfTransactionsThreshold
        {
            get
            {
                return  _appSettingsHelper.GetInt(
                    AppSettingsKey.TokenAccountSnapshotNumberOfTransactionsThreshold,
                    _appSettingsDefaultValue.TokenAccountSnapshotNumberOfTransactionsThreshold);
            }
        }
    }
}
