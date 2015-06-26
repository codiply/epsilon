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

        public SmtpService(
            ISmtpClientWrapperFactory smtpClientWrapperFactory)
        {
            _smtpClientWrapperFactory = smtpClientWrapperFactory;
        }

        public void Send(MailMessage message)
        {
            var client = _smtpClientWrapperFactory.CreateSmtpClientWrapper();

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            client.Port = 465;

            // setup Smtp authentication
            System.Net.NetworkCredential credentials =
                new System.Net.NetworkCredential("email@gmail.com", "password");
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            message.From = new MailAddress("email@gmail.com", "Name");

            client.Send(message);
        }
    }
}
