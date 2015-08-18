using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class SmtpServiceTest
    {
        private ISmtpClientWrapper _clientUsed;

        [Test]
        public void Send_WithAllowThrowExceptionFalse_DoesNotThrowButLogsException()
        {
            var message = new MailMessage();
            var allowThrowException = false;

            Exception loggedException = null;
            var elmahHelper = CreateElmahHelper((ex) => loggedException = ex);
            var service = new SmtpService(null, null, elmahHelper);

            Assert.DoesNotThrow(() => service.Send(message, allowThrowException),
                "Calling Send should not throw.");
            Assert.IsNotNull(loggedException, "The exception was not logged.");
        }

        [Test]
        public void Send_WithAllowThrowExceptionTrue_DoesNotThrowButLogsException()
        {
            var message = new MailMessage();
            var allowThrowException = true;

            Exception loggedException = null;
            var elmahHelper = CreateElmahHelper((ex) => loggedException = ex);
            var service = new SmtpService(null, null, elmahHelper);

            Assert.Throws<NullReferenceException>(() => service.Send(message, allowThrowException),
                "Calling Send should throw.");
            Assert.IsNull(loggedException, "The exception should not be logged.");
        }

        [Test]
        public void Send_Test()
        {
            var host = "host";
            var userName = "user-name";
            var password = "password";
            var fromAddress = "from@test.com";
            var fromDisplayName = "From Display Name";
            var port = 1234;
            var timeoutMilliseconds = 999;
            var enableSsl = true;

            var messageToSend = new MailMessage();

            Exception loggedException = null;
            MailMessage messageSent = null;

            var config = CreateConfig(
                host: host, userName: userName, password: password, fromAddress: fromAddress,
                fromDisplayName: fromDisplayName, port: port, timeoutMilliseconds: timeoutMilliseconds, enableSsl: enableSsl);
            var clientFactory = CreateClientFactory(x => messageSent = x);
            var elmahHelper = CreateElmahHelper((ex) => loggedException = ex);

            var service = new SmtpService(config, clientFactory, elmahHelper);

            service.Send(messageToSend, allowThrowException: false);

            Assert.IsNotNull(_clientUsed, "The client was null.");
            Assert.AreEqual(host, _clientUsed.Host, "Host on the client was not the expected.");
            Assert.AreEqual(port, _clientUsed.Port, "Port on the client was not the expected.");
            Assert.AreEqual(timeoutMilliseconds, _clientUsed.Timeout, "Timeout on the client was not the expected.");
            Assert.AreEqual(enableSsl, _clientUsed.EnableSsl, "EnableSsl on the client was not the expected.");

            Assert.AreSame(messageToSend, messageSent, "The same message was not passed on to the client.");

            Assert.IsNull(loggedException, "No exception should not be logged.");
        }

        private ISmtpClientWrapperFactory CreateClientFactory(Action<MailMessage> clientSendCallback)
        {
            var mockFactory = new Mock<ISmtpClientWrapperFactory>();

            mockFactory.Setup(x => x.CreateSmtpClientWrapper()).Returns(() => CreateClient(clientSendCallback));

            return mockFactory.Object;
        }

        private ISmtpClientWrapper CreateClient(Action<MailMessage> clientSendCallback)
        {
            var mockClient = new Mock<ISmtpClientWrapper>();

            mockClient.SetupAllProperties();
            mockClient.Setup(x => x.Send(It.IsAny<MailMessage>())).Callback(clientSendCallback);

            _clientUsed = mockClient.Object;

            return mockClient.Object;
        }

        private ISmtpServiceConfig CreateConfig(
            string host, string userName, string password, string fromAddress, string fromDisplayName,
            int port, int timeoutMilliseconds, bool enableSsl)
        {
            var mockConfig = new Mock<ISmtpServiceConfig>();

            mockConfig.Setup(x => x.Host).Returns(host);
            mockConfig.Setup(x => x.UserName).Returns(userName);
            mockConfig.Setup(x => x.Password).Returns(password);
            mockConfig.Setup(x => x.FromAddress).Returns(fromAddress);
            mockConfig.Setup(x => x.FromDisplayName).Returns(fromDisplayName);
            mockConfig.Setup(x => x.Port).Returns(port);
            mockConfig.Setup(x => x.TimeoutMilliseconds).Returns(timeoutMilliseconds);
            mockConfig.Setup(x => x.EnableSsl).Returns(enableSsl);

            return mockConfig.Object;
        }

        private IElmahHelper CreateElmahHelper(Action<Exception> elmahHelperRaiseHandler)
        {
            var mockElmahHelper = new Mock<IElmahHelper>();
            mockElmahHelper.Setup(x => x.Raise(It.IsAny<Exception>())).Callback(elmahHelperRaiseHandler);

            return mockElmahHelper.Object;
        }
    }
}
