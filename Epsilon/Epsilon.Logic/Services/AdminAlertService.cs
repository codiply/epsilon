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

namespace Epsilon.Logic.Services
{
    public class AdminAlertService : IAdminAlertService
    {
        private static ConcurrentDictionary<string, object> _locks =
            new ConcurrentDictionary<string, object>();

        private readonly IClock _clock;
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;
        private readonly IEpsilonContext _dbContext;
        private readonly ISmtpService _smtpService;

        private TimeSpan? _snoozePeriodInHours;

        public AdminAlertService(
            IClock clock,
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue,
            IEpsilonContext dbContext,
            ISmtpService smtpService)
        {
            _clock = clock;
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
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

            return timeElapsed < SnoozePeriod();
        }

        private void DoSendAlert(string key)
        {
            var message = new MailMessage
            {
                Subject = "Epsilon AdminAlert: " + key,
                Body = key,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress("recepient@gmail.com", "Recepient"));
            _smtpService.Send(message);
        }

        private void RecordAlertSent(string key)
        {
            _dbContext.AdminAlerts.Add(new AdminAlert
            {
                Key = key,
                SentOn = _clock.OffsetNow
            });
            _dbContext.SaveChanges();
        }

        private TimeSpan SnoozePeriod()
        {
            if (_snoozePeriodInHours == null)
            {
                var value = _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.AdminAlertSnoozePeriodInHours, 
                    _dbAppSettingDefaultValue.AdminAlertSnoozePeriodInHours);
                _snoozePeriodInHours = TimeSpan.FromHours(value);
            }
            
            return _snoozePeriodInHours.Value;
        }

        private static object GetLock(string key)
        {
            return _locks.GetOrAdd(key, x => new Object());
        }
    }
}
