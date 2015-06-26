using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface ISmtpClientWrapper
    {
        ICredentialsByHost Credentials { get; set; }
        SmtpDeliveryMethod DeliveryMethod { get; set; }
        bool EnableSsl { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        bool UseDefaultCredentials { get; set; }

        void Send(MailMessage message);
    }
}
