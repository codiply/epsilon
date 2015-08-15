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
using Epsilon.Logic.Infrastructure.Interfaces;

namespace Epsilon.Logic.Services
{
    public class AdminAlertService : IAdminAlertService
    {
        private static ConcurrentDictionary<string, object> _locks =
            new ConcurrentDictionary<string, object>();

        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IAdminAlertServiceConfig _adminAlertServiceConfig;
        private readonly IEpsilonContext _dbContext;
        private readonly ISmtpService _smtpService;
        private readonly IElmahHelper _elmahHelper;

        public AdminAlertService(
            IClock clock,
            IAppCache appCache,
            IAdminAlertServiceConfig adminAlertServiceConfig,
            IEpsilonContext dbContext,
            ISmtpService smtpService,
            IElmahHelper elmahHelper)
        {
            _clock = clock;
            _appCache = appCache;
            _adminAlertServiceConfig = adminAlertServiceConfig;
            _dbContext = dbContext;
            _smtpService = smtpService;
            _elmahHelper = elmahHelper;
        }

        // TODO_TEST_PANOS
        public void SendAlert(string key, bool doNotUseDatabase = false)
        {
            try
            {
                if (IsNotAllowedToSendAgain(key, doNotUseDatabase))
                    return;

                lock (GetLock(key))
                {
                    if (IsNotAllowedToSendAgain(key, doNotUseDatabase))
                        return;

                    DoSendAlert(key);
                    RecordAlertSent(key, doNotUseDatabase);
                }
            }
            catch (Exception ex)
            {
                _elmahHelper.Raise(ex);
                if (doNotUseDatabase == false)
                {
                    SendAlert(key, doNotUseDatabase: true);
                }
            }
        }

        // TODO_TEST_PANOS
        private bool IsNotAllowedToSendAgain(string key, bool doNotUseDatabase)
        {
            var appCacheContainsKey = _appCache.ContainsKey(AppCacheKey.AdminAlertSent(key)); 
            if (appCacheContainsKey)
                return true;

            if (doNotUseDatabase)
            {
                return appCacheContainsKey; 
            }
            else
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
        }

        private void DoSendAlert(string key)
        {
            var applicationName = _adminAlertServiceConfig.ApplicationName;
            var message = new MailMessage
            {
                Subject = string.Format("{0} AdminAlert: {1}", applicationName, key),
                Body = string.Format("This is an AdminAlert with key <strong>{0}</strong> from application <strong>{1}</strong>.", key, applicationName),
                IsBodyHtml = true
            };
            var emailList = _adminAlertServiceConfig.EmailList;
            var emails = emailList.Trim(';', ',').Split(';', ',').Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
            foreach (var em in emails)
            {
                message.To.Add(new MailAddress(em, em));
            }
            _smtpService.Send(message, allowThrowException: true);
        }

        private void RecordAlertSent(string key, bool doNotUseDatabase)
        {
            // TODO_TEST_PANOS
            var value = _appCache.Get(AppCacheKey.AdminAlertSent(key),
                () => "value-is-irrelevant", _adminAlertServiceConfig.SnoozePeriod, WithLock.No);

            if (doNotUseDatabase == false)
            {
                _dbContext.AdminAlerts.Add(new AdminAlert
                {
                    Key = key
                });
                _dbContext.SaveChanges();
            }
        }

        private static object GetLock(string key)
        {
            return _locks.GetOrAdd(key, x => new Object());
        }
    }
}
