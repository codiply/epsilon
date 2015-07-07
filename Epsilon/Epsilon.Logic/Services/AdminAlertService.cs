using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Constants.Interfaces;
using System.Net.Mail;
using Epsilon.Logic.Configuration.Interfaces;

namespace Epsilon.Logic.Services
{
    public class AdminAlertService : IAdminAlertService
    {
        private static ConcurrentDictionary<string, object> _locks =
            new ConcurrentDictionary<string, object>();

        private readonly IClock _clock;
        private readonly IAdminAlertServiceConfig _adminAlertServiceConfig;
        private readonly IEpsilonContext _dbContext;
        private readonly ISmtpService _smtpService;

        private TimeSpan? _snoozePeriodInHours;

        public AdminAlertService(
            IClock clock,
            IAdminAlertServiceConfig adminAlertServiceConfig,
            IEpsilonContext dbContext,
            ISmtpService smtpService)
        {
            _clock = clock;
            _adminAlertServiceConfig = adminAlertServiceConfig;
            _dbContext = dbContext;
            _smtpService = smtpService;
        }


        public void SendAlert(string key)
        {
            if (IsNotAllowedToSendAgain(key))
                return;

            lock (GetLock(key))
            {
                if (IsNotAllowedToSendAgain(key))
                    return;

                DoSendAlert(key);
                RecordAlertSent(key);
            }
        }

        private bool IsNotAllowedToSendAgain(string key)
        {
            var latestAlertSent = 
                _dbContext.AdminAlerts
                .Where(x => x.Key.Equals(key))
                .OrderByDescending(x => x.SentOn)
                .FirstOrDefault();

            if (latestAlertSent == null)
                return false;

            var timeElapsed = _clock.OffsetNow - latestAlertSent.SentOn;

            return timeElapsed < _adminAlertServiceConfig.SnoozePeriod;
        }

        private void DoSendAlert(string key)
        {
            var applicationName = _adminAlertServiceConfig.ApplicationName;
            var message = new MailMessage
            {
                Subject = String.Format("{0} AdminAlert: {1}", applicationName, key),
                Body = String.Format("This is an AdminAlert with key <strong>{0}</strong> from application <strong>{1}</strong>.", key, applicationName),
                IsBodyHtml = true
            };
            var emailList = _adminAlertServiceConfig.EmailList;
            var emails = emailList.Trim(';', ',').Split(';', ',').Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
            foreach (var em in emails)
            {
                message.To.Add(new MailAddress(em, em));
            }
            _smtpService.Send(message);
        }

        private void RecordAlertSent(string key)
        {
            _dbContext.AdminAlerts.Add(new AdminAlert
            {
                Key = key
            });
            _dbContext.SaveChanges();
        }

        private static object GetLock(string key)
        {
            return _locks.GetOrAdd(key, x => new Object());
        }
    }
}
