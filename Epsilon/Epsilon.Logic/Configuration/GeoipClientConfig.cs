using Epsilon.Logic.Configuration.Interfaces;
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
    public class GeoipClientConfig : IGeoipClientConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public GeoipClientConfig(
           IDbAppSettingsHelper dbAppSettingsHelper,
           IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public double TimeoutInMilliseconds
        {
            get
            {
                return _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.GeoipClient_TimeoutInMilliseconds,
                    _dbAppSettingDefaultValue.GeoipClient_TimeoutInMilliseconds);
            }
        }
    }
}
