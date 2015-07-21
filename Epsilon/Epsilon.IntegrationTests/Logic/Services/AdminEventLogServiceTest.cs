using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.Data.Entity;
using Epsilon.IntegrationTests.BaseFixtures;
using NUnit.Framework;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;

namespace Epsilon.IntegrationTests.Logic.Services
{
    class AdminEventLogServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task Log_CreatesLogEntryAndSetsRecordedOn()
        {
            var key = AdminEventLogKey.GeocodeServiceRetryDueToOverQueryLimit;
            var keyToString = EnumsHelper.AdminEventLogKey.ToString(key);
            var extraInfo = "extra-info";

            var container = CreateContainer();
            var service = container.Get<IAdminEventLogService>();

            var timeBefore = DateTimeOffset.Now;
            await service.Log(key, extraInfo);

            var retrievedAdminEventLog = await DbProbe.AdminEventLogs
                .SingleOrDefaultAsync(x => x.RecordedOn >= timeBefore);

            var timeAFter = DateTimeOffset.Now;

            Assert.IsNotNull(retrievedAdminEventLog, "An AdminEventLog was not found in the database.");
            Assert.AreEqual(keyToString, retrievedAdminEventLog.Key, "The Key field is not the expected.");
            Assert.AreEqual(extraInfo, retrievedAdminEventLog.ExtraInfo, "The ExtraInfo field is not the expected.");
            Assert.IsTrue(timeBefore <= retrievedAdminEventLog.RecordedOn && retrievedAdminEventLog.RecordedOn <= timeAFter,
                "The RecordedOn field is not within the expected range.");
        }

    }
}
