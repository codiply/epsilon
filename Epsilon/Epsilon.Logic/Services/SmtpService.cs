using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Net.Mail;

namespace Epsilon.Logic.Services
{
    public class SmtpService : ISmtpService
    {
        private readonly ISmtpServiceConfig _smtpServiceConfig;
        private readonly ISmtpClientWrapperFactory _smtpClientWrapperFactory;
        private readonly IElmahHelper _elmahHelper;

        public SmtpService(
            ISmtpServiceConfig smtpServiceConfig,
            ISmtpClientWrapperFactory smtpClientWrapperFactory,
            IElmahHelper elmahHelper)
        {
            _smtpServiceConfig = smtpServiceConfig;
            _smtpClientWrapperFactory = smtpClientWrapperFactory;
            _elmahHelper = elmahHelper;
        }

        public void Send(MailMessage message, bool allowThrowException)
        {
            try {
                var client = _smtpClientWrapperFactory.CreateSmtpClientWrapper();

                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Host = _smtpServiceConfig.Host;
                client.Port = _smtpServiceConfig.Port;
                client.Timeout = _smtpServiceConfig.TimeoutMilliseconds;

                client.EnableSsl = _smtpServiceConfig.EnableSsl;

                var userName = _smtpServiceConfig.UserName;
                var password = _smtpServiceConfig.Password;

                System.Net.NetworkCredential credentials =
                    new System.Net.NetworkCredential(userName, password);
                client.UseDefaultCredentials = false;
                client.Credentials = credentials;

                var fromAddress = _smtpServiceConfig.FromAddress;
                var fromDisplayName = _smtpServiceConfig.FromDisplayName;

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
