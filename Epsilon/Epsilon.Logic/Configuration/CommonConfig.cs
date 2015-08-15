using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using System;

namespace Epsilon.Logic.Configuration
{
    public class CommonConfig : ICommonConfig
    {
        public IAppSettingsDefaultValue _appSettingsDefaultValue;
        public IAppSettingsHelper _appSettingsHelper;

        public CommonConfig(
            IAppSettingsDefaultValue appSettingsDefaultValue,
            IAppSettingsHelper appSettingsHelper)
        {
            _appSettingsDefaultValue = appSettingsDefaultValue;
            _appSettingsHelper = appSettingsHelper;
        }

        public TimeSpan DefaultAppCacheSlidingExpiration
        {
            get
            {
                return _appSettingsHelper.GetTimeSpan(
                    AppSettingsKey.DefaultAppCacheSlidingExpiration,
                    _appSettingsDefaultValue.DefaultAppCacheSlidingExpiration);
            }
        }
    }
}
