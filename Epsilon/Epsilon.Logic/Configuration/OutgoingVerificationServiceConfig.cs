using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
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

        public OutgoingVerificationServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
        }

        public bool GlobalSwitch_DisablePickOutgoingVerification
        {
            get
            {
                return _dbAppSettingsHelper.GetBool(EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.GlobalSwitch_DisablePickOutgoingVerification)) == true;
            }
        }
    }
}
