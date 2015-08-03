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
    public class UserTokenServiceConfig : IUserTokenServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public UserTokenServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public int MyTokenTransactions_PageSize
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.Token_MyTokenTransactions_PageSize),
                    _dbAppSettingDefaultValue.Token_MyTokenTransactions_PageSize);
            }
        }
    }
}
