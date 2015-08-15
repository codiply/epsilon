using System.Net.Mail;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ISmtpService
    {
        void Send(MailMessage message, bool allowThrowException);
    }
}
