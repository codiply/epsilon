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
            var key = AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitSuccessAfterRetrying;
            var keyToString = EnumsHelper.AdminEventLogKey.ToString(key);
            var extraInfokey1 = "key1";
            var extraInfokey2 = "key2";
            var extraInfovalue1 = "value1";
            var extraInfovalue2 = 2;
            var extraInfo = new Dictionary<string, object>
            {
                { extraInfokey1, extraInfovalue1 },
                { extraInfokey2, extraInfovalue2 }
            };

            var container = CreateContainer();
            var service = container.Get<IAdminEventLogService>();
            
            var timeBefore = DateTimeOffset.Now;
            await service.Log(key, extraInfo);

            var retrievedAdminEventLog = await DbProbe.AdminEventLogs
                .SingleOrDefaultAsync(x => x.RecordedOn >= timeBefore);

            var timeAFter = DateTimeOffset.Now;

            Assert.IsNotNull(retrievedAdminEventLog, "An AdminEventLog was not found in the database.");
            Assert.AreEqual(keyToString, retrievedAdminEventLog.Key, "The Key field is not the expected.");
            Assert.IsTrue(timeBefore <= retrievedAdminEventLog.RecordedOn && retrievedAdminEventLog.RecordedOn <= timeAFter,
                "The RecordedOn field is not within the expected range.");

            var extraInfoDict = retrievedAdminEventLog.ExtraInfoDict();
            Assert.IsTrue(extraInfoDict.ContainsKey(extraInfokey1), "Key1 was not found in the retrieved dictionary for ExtraInfo");
            Assert.AreEqual(extraInfovalue1, extraInfoDict[extraInfokey1], "Value for Key1 in ExtraInfo is not the expected.");
            Assert.IsTrue(extraInfoDict.ContainsKey(extraInfokey2), "Key2 was not found in the retrieved dictionary for ExtraInfo");
            Assert.AreEqual(extraInfovalue2, extraInfoDict[extraInfokey2], "Value for Key2 in ExtraInfo is not the expected.");
        }

        [Test]
        public async Task Log_CanHandleNullExtraInfo()
        {
            var key = AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitSuccessAfterRetrying;
            var keyToString = EnumsHelper.AdminEventLogKey.ToString(key);
            Dictionary<string, object> extraInfo = null;

            var container = CreateContainer();
            var service = container.Get<IAdminEventLogService>();

            var timeBefore = DateTimeOffset.Now;
            await service.Log(key, extraInfo);

            var retrievedAdminEventLog = await DbProbe.AdminEventLogs
                .SingleOrDefaultAsync(x => x.RecordedOn >= timeBefore);

            var timeAFter = DateTimeOffset.Now;

            Assert.IsNotNull(retrievedAdminEventLog, "An AdminEventLog was not found in the database.");
            Assert.AreEqual(keyToString, retrievedAdminEventLog.Key, "The Key field is not the expected.");
            Assert.IsTrue(timeBefore <= retrievedAdminEventLog.RecordedOn && retrievedAdminEventLog.RecordedOn <= timeAFter,
                "The RecordedOn field is not within the expected range.");

            var extraInfoDict = retrievedAdminEventLog.ExtraInfoDict();
            Assert.IsTrue(extraInfoDict.Count == 0, "The ExtraInfo dictionary should be empty");
        }
    }
}
