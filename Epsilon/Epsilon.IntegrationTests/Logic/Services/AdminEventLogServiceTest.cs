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
            var key = AdminEventLogKey.GooglGeocodeApiStatusOverQueryLimitSuccessAfterRetrying;
            var keyToString = EnumsHelper.AdminEventLogKey.ToString(key);
            var key1 = "key1";
            var key2 = "key2";
            var value1 = "value1";
            var value2 = 2;
            var extraInfo = new Dictionary<string, object> { { key1, value1 }, { key2, value2 } };

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
            Assert.IsTrue(extraInfoDict.ContainsKey(key1), "Key1 was not found in the retrieved dictionary for ExtraInfo");
            Assert.AreEqual(value1, extraInfoDict[key1], "Value for Key1 in ExtraInfo is not the expected.");
            Assert.IsTrue(extraInfoDict.ContainsKey(key2), "Key2 was not found in the retrieved dictionary for ExtraInfo");
            Assert.AreEqual(value2, extraInfoDict[key2], "Value for Key2 in ExtraInfo is not the expected.");
        }

    }
}
