using Epsilon.IntegrationTests.BaseFixtures;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.IntegrationTests.ReferenceData
{
    // !!! IMPORTANT !!!
    // If you fix a test here by changing a value in the database, you might need
    // to restart your NUnit test runner as DbAppSettingsHelper is caching values.

    public class DbAppSetting_AntiAbuseTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public void PickOutgoingVerificationRules()
        {
            var container = CreateContainer();
            var helper = container.Get<IDbAppSettingsHelper>();

            var maxOutstandingPerUserForNewUser = helper
                .GetInt(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingPerUserForNewUser);
            Assert.IsTrue(maxOutstandingPerUserForNewUser >= 4,
                String.Format("The MaxOutstandingPerUserForNewUser ({0}) must be greater or equal to 4 as some of the verifications might fail on the recipient side.",
                maxOutstandingPerUserForNewUser));

            var maxOutstandingPerUser = helper
                .GetInt(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingPerUser);
            Assert.IsTrue(maxOutstandingPerUser >= maxOutstandingPerUserForNewUser,
                String.Format("The MaxOutstandingPerUser ({0}) must be greater or equal to MaxOutstandingPerUserForNewUser ({1}).",
                maxOutstandingPerUser, maxOutstandingPerUserForNewUser));

            var maxFrequencyPerIpAddress = helper
                .GetFrequency(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress);
            Assert.IsTrue(maxFrequencyPerIpAddress.Times >= maxOutstandingPerUser,
                String.Format("The Times field ({0}) on the frequency MaxOutstandingPerUserConstant must be greater or equal to the MaxOutstandingPerUser ({1}).",
                maxFrequencyPerIpAddress.Times, maxOutstandingPerUser));

            var myOutgoingVerificationsSummaryItemsLimit = helper
                .GetInt(DbAppSettingKey.OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit);
            Assert.IsTrue(maxOutstandingPerUser <= myOutgoingVerificationsSummaryItemsLimit,
                String.Format("The MaxOutstandingPerUserForNewUser ({0}) should be less or equal to MyOutgoingVerificationsSummaryItemsLimit ({1}) so that all outstanding outgoing verifications can appear on the front page.",
                maxOutstandingPerUserForNewUser, myOutgoingVerificationsSummaryItemsLimit));
        }

        public void UserMaxFrequencyTimesShouldBeLessOrEqualToCorrespondingIpAddressMaxFrequencyTimes()
        {
        }
    }
}
