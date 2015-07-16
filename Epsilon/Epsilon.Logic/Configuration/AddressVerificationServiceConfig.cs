using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration
{
    public class AddressVerificationServiceConfig : IAddressVerificationServiceConfig
    {
        public IAppSettingsHelper _appSettingsHelper;

        public AddressVerificationServiceConfig(
            IAppSettingsHelper appSettingsHelper)
        {
            _appSettingsHelper = appSettingsHelper;
        }

        public string GoogleApiServerKey
        {
            get
            {
                return _appSettingsHelper.GetString(AppSettingsKey.GoogleApiServerKey);
            }
        }
    }
}
