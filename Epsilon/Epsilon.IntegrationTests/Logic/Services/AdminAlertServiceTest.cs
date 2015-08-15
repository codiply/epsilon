using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AdminAlertServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task SendAlert_SendsOnlyOncePerSnoozePeriod()
        {
            var applicationName = "TestApplication";
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var email3 = "test3@test.com";
            var emailList = string.Format("{0};{1} , {2};", email1, email2, email3);
            var snoozePeriod = TimeSpan.FromDays(1);
            var adminAlertKey = "Test-Admin-Alert-Key";

            var container = CreateContainer();
     
            SetupConfig(container, applicationName, emailList, snoozePeriod);

            MailMessage mailMessage = null;
            SetupSmtpService(container, (x, allowThrow) => mailMessage = x); // TODO_TEST_PANOS: check allowThrow

            var service = container.Get<IAdminAlertService>();

            var timeBefore = DateTimeOffset.Now;
            service.SendAlert(adminAlertKey);
            var timeAfter = DateTimeOffset.Now;

            var retrievedAdminAlert = await DbProbe.AdminAlerts.SingleOrDefaultAsync(x => x.Key == adminAlertKey);

            Assert.IsNotNull(retrievedAdminAlert, "An admin alert was not recorded in the database.");
            Assert.AreEqual(adminAlertKey, retrievedAdminAlert.Key, "The key on the AdminAlert is not the expected.");
            Assert.IsTrue(timeBefore <= retrievedAdminAlert.SentOn && retrievedAdminAlert.SentOn <= timeAfter,
                "The field SentOn on the AdminAlert has unexpected value.");

            Assert.IsNotNull(mailMessage, "A MailMessage was not sent using the SmtpService.");
            Assert.IsTrue(mailMessage.Subject.Contains(applicationName) && mailMessage.Body.Contains(applicationName),
                "The mail message subject and body should contain the application name.");
            Assert.IsTrue(mailMessage.Subject.Contains(adminAlertKey) && mailMessage.Body.Contains(adminAlertKey),
                "The mail message subject and body should contain the Admin Alert key.");
            Assert.IsTrue(mailMessage.To.Any(x => x.Address.Equals(email1)), "Email1 was not found in the recepients.");
            Assert.IsTrue(mailMessage.To.Any(x => x.Address.Equals(email2)), "Email2 was not found in the recepients.");
            Assert.IsTrue(mailMessage.To.Any(x => x.Address.Equals(email3)), "Email3 was not found in the recepients.");

            // I set the mailMessage to null and send the AdminAlert again.
            mailMessage = null;
            service.SendAlert(adminAlertKey);

            Assert.IsNull(mailMessage, "No email should be sent the second time SendAlert is called.");

            var newAdminAlertCount = await DbProbe.AdminAlerts.Where(x => x.SentOn > timeAfter).CountAsync();
            Assert.AreEqual(0, newAdminAlertCount, 
                "No AdminAlert record should be recorded the second tim SendAlert is called.");
        }

        [Test]
        public async Task SendAlert_SendsAgainAfterSnoozePeriodIsOver()
        {
            var applicationName = "TestApplication";
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var email3 = "test3@test.com";
            var emailList = string.Format("{0};{1} , {2};", email1, email2, email3);
            var snoozePeriodInSeconds = 0.2;
            var smallDelay = TimeSpan.FromSeconds(snoozePeriodInSeconds / 100);
            var snoozePeriod = TimeSpan.FromSeconds(snoozePeriodInSeconds);
            var adminAlertKey = "Test-Admin-Alert-Key";

            var container = CreateContainer();

            SetupConfig(container, applicationName, emailList, snoozePeriod);

            MailMessage mailMessage1 = null;
            SetupSmtpService(container, (x, allowThrow) => mailMessage1 = x); // TODO_TEST_PANOS: check allowThrow
            var service1 = container.Get<IAdminAlertService>();

            var time1 = DateTimeOffset.Now;
            await Task.Delay(smallDelay);
            service1.SendAlert(adminAlertKey);
            await Task.Delay(smallDelay);
            var time2 = DateTimeOffset.Now;

            MailMessage mailMessage2 = null;
            SetupSmtpService(container, (x, allowThrow) => mailMessage2 = x); // TODO_TEST_PANOS: check allowThrow
            var service2 = container.Get<IAdminAlertService>();

            service2.SendAlert(adminAlertKey);
            await Task.Delay(smallDelay);
            var time3 = DateTimeOffset.Now;

            await Task.Delay(snoozePeriod);

            MailMessage mailMessage3 = null;
            SetupSmtpService(container, (x, allowThrow) => mailMessage3 = x); // TODO_TEST_PANOS: check allowThrow
            var service3 = container.Get<IAdminAlertService>();

            service3.SendAlert(adminAlertKey);
            await Task.Delay(smallDelay);
            var time4 = DateTimeOffset.Now;

            var retrievedAdminAlert1 = await DbProbe.AdminAlerts
                .SingleOrDefaultAsync(x => x.Key == adminAlertKey && time1 <= x.SentOn && x.SentOn <= time2);
            var retrievedAdminAlert2 = await DbProbe.AdminAlerts
                .SingleOrDefaultAsync(x => x.Key == adminAlertKey && time2 <= x.SentOn && x.SentOn <= time3);
            var retrievedAdminAlert3 = await DbProbe.AdminAlerts
                .SingleOrDefaultAsync(x => x.Key == adminAlertKey && time3 <= x.SentOn && x.SentOn <= time4);

            Assert.IsNotNull(retrievedAdminAlert1, "An AdminAlert record should be recorded the first time.");
            Assert.IsNotNull(mailMessage1, "An email should be sent the first time.");
            Assert.IsNull(retrievedAdminAlert2, "An AdminAlert record should not be recorded the second time.");
            Assert.IsNull(mailMessage2, "An email should be sent the second time.");
            Assert.IsNotNull(retrievedAdminAlert3, "An AdminAlert record should be recorded the third time.");
            Assert.IsNotNull(mailMessage3, "An email should be sent the third time.");
        }

        private static void SetupSmtpService(IKernel container, Action<MailMessage, bool> callback)
        {
            var mockSmtpService = new Mock<ISmtpService>();
            mockSmtpService.Setup(x => x.Send(It.IsAny<MailMessage>(), It.IsAny<bool>())).Callback(callback);

            container.Rebind<ISmtpService>().ToConstant(mockSmtpService.Object);
        }

        private static void SetupConfig(IKernel container, string applicationName, 
            string emailList, TimeSpan snoozePeriod)
        {
            var mockConfig = new Mock<IAdminAlertServiceConfig>();
            mockConfig.Setup(x => x.ApplicationName).Returns(applicationName);
            mockConfig.Setup(x => x.EmailList).Returns(emailList);
            mockConfig.Setup(x => x.SnoozePeriod).Returns(snoozePeriod);

            container.Rebind<IAdminAlertServiceConfig>().ToConstant(mockConfig.Object);
        }
    }
}
