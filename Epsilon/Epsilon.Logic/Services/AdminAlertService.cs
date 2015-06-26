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

        private TimeSpan? _snoozePeriodInHours;

        public AdminAlertService(
            IClock clock,
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
            _dbContext = dbContext;
        }


        public void Send(string key)
        {
            if (IsNotAllowedToSendAgain(key))
                return;

            lock (GetLock(key))
            {
                if (IsNotAllowedToSendAgain(key))
                    return;

                DoSend(key);
                RecordAlertSent(key);
            }
        }

        private bool IsNotAllowedToSendAgain(string key)
        {
            var latestAlertSent = _dbContext.AdminAlerts.OrderByDescending(x => x.SentOn).FirstOrDefault();

            if (latestAlertSent == null)
                return false;

            var timeElapsed = _clock.OffsetNow - latestAlertSent.SentOn;

            return timeElapsed < SnoozePeriod();
        }

        private void DoSend(string key)
        {
            // TODO: Send the message in here.
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
