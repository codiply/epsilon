using Epsilon.Logic.Configuration.Interfaces;
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
    public class SmtpServiceConfig : ISmtpServiceConfig
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IAppSettingsDefaultValue _appSettingsDefaultValue;

        public SmtpServiceConfig(
            IAppSettingsHelper appSettingsHelper,
            IAppSettingsDefaultValue appSettingsDefaultValue)
        {
            _appSettingsHelper = appSettingsHelper;
            _appSettingsDefaultValue = appSettingsDefaultValue;
        }

        public string Host
        {
            get {
                return _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceHost);
            }
        }

        public string UserName
        {
            get
            {
                return _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceUserName);
            }
        }

        public string Password
        {
            get
            {
                return _appSettingsHelper.GetString(AppSettingsKey.SmtpServicePassword);
            }
        }

        public string FromAddress
        {
            get
            {
                return _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceFromAddress);
            }
        }

        public string FromDisplayName
        {
            get
            {
                return _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceFromDisplayName);
            }
        }

        public int Port
        {
            get
            {
                return _appSettingsHelper.GetInt(
                    AppSettingsKey.SmtpServicePort, 
                    _appSettingsDefaultValue.SmtpServicePort);
            }
        }

        public int TimeoutMilliseconds
        {
            get
            {
                return _appSettingsHelper.GetInt(
                    AppSettingsKey.SmtpServiceTimeoutMilliseconds,
                    _appSettingsDefaultValue.SmtpServiceTimeoutMilliseconds);
            }
        }

        public bool EnableSsl
        {
            get
            {
                return _appSettingsHelper.GetBool(
                    AppSettingsKey.SmtpServiceEnableSsl,
                    _appSettingsDefaultValue.SmtpServiceEnableSsl);
            }
        }
    }
}
