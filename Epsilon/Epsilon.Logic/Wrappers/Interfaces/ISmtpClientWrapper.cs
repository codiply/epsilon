using System.Net;
using System.Net.Mail;

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
        int Timeout { get; set; }

        void Send(MailMessage message);
    }
}
