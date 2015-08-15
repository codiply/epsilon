using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class SmtpService : ISmtpService
    {
        private readonly ISmtpClientWrapperFactory _smtpClientWrapperFactory;
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IElmahHelper _elmahHelper;

        public SmtpService(
            ISmtpClientWrapperFactory smtpClientWrapperFactory,
            IAppSettingsHelper appSettingsHelper,
            IElmahHelper elmahHelper)
        {
            _smtpClientWrapperFactory = smtpClientWrapperFactory;
            _appSettingsHelper = appSettingsHelper;
            _elmahHelper = elmahHelper;
        }

        public void Send(MailMessage message, bool allowThrowException)
        {
            try {
                var client = _smtpClientWrapperFactory.CreateSmtpClientWrapper();

                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Host = _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceHost);
                client.Port = _appSettingsHelper.GetInt(AppSettingsKey.SmtpServicePort).Value;
                client.Timeout = _appSettingsHelper.GetInt(AppSettingsKey.SmtpServiceTimeoutMilliseconds).Value;

                client.EnableSsl = _appSettingsHelper.GetBool(AppSettingsKey.SmtpServiceEnableSsl).Value;

                var userName = _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceUserName);
                var password = _appSettingsHelper.GetString(AppSettingsKey.SmtpServicePassword);

                System.Net.NetworkCredential credentials =
                    new System.Net.NetworkCredential(userName, password);
                client.UseDefaultCredentials = false;
                client.Credentials = credentials;

                var fromAddress = _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceFromAddress);
                var fromDisplayName = _appSettingsHelper.GetString(AppSettingsKey.SmtpServiceFromDisplayName);

                message.From = new MailAddress(fromAddress, fromDisplayName);

                client.Send(message);
            }
            catch (Exception ex)
            {
                if (allowThrowException)
                    throw ex;
                else
                    _elmahHelper.Raise(ex);
            }
        }
    }
}
