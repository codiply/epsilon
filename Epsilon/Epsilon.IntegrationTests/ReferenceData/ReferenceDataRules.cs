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

    public class ReferenceDataRules : BaseIntegrationTestWithRollback
    {
        [Test]
        public void AntiAbuse_PickOutgoingVerificationRules()
        {
            var container = CreateContainer();
            var helper = container.Get<IDbAppSettingsHelper>();

            var maxOutstandingPerUserForNewUser = helper
                .GetInt(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingPerUserForNewUser);
            Assert.IsTrue(maxOutstandingPerUserForNewUser.HasValue, "MaxOutstandingPerUserForNewUser has no value.");
            Assert.IsTrue(maxOutstandingPerUserForNewUser.Value >= 4,
                String.Format("The MaxOutstandingPerUserForNewUser ({0}) must be greater or equal to 4 as some of the verifications might fail on the recipient side.",
                maxOutstandingPerUserForNewUser));

            var maxOutstandingPerUser = helper
                .GetInt(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingPerUser);
            Assert.IsTrue(maxOutstandingPerUser.HasValue, "MaxOutstandingPerUser has no value.");
            Assert.IsTrue(maxOutstandingPerUser.Value >= maxOutstandingPerUserForNewUser.Value,
                String.Format("The MaxOutstandingPerUser ({0}) must be greater or equal to MaxOutstandingPerUserForNewUser ({1}).",
                maxOutstandingPerUser, maxOutstandingPerUserForNewUser));

            var maxFrequencyPerIpAddress = helper
                .GetFrequency(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress);
            Assert.IsNotNull(maxFrequencyPerIpAddress, "MaxFrequencyPerIpAddress has no value.");
            Assert.IsTrue(maxFrequencyPerIpAddress.Times >= maxOutstandingPerUser.Value,
                String.Format("The Times field ({0}) on the frequency MaxOutstandingPerUserConstant must be greater or equal to the MaxOutstandingPerUser ({1}).",
                maxFrequencyPerIpAddress.Times, maxOutstandingPerUser));

            var myOutgoingVerificationsSummaryItemsLimit = helper
                .GetInt(DbAppSettingKey.OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit);
            Assert.IsTrue(myOutgoingVerificationsSummaryItemsLimit.HasValue, "MyOutgoingVerificationsSummaryItemsLimit has no value.");
            Assert.IsTrue(maxOutstandingPerUser.Value <= myOutgoingVerificationsSummaryItemsLimit.Value,
                String.Format("The MaxOutstandingPerUserForNewUser ({0}) should be less or equal to MyOutgoingVerificationsSummaryItemsLimit ({1}) so that all outstanding outgoing verifications can appear on the front page.",
                maxOutstandingPerUserForNewUser, myOutgoingVerificationsSummaryItemsLimit));
        }

        [Test]
        public void AntiAbuse_UserMaxFrequency_vs_IpAddressMaxFrequency_Rules()
        {
            var container = CreateContainer();
            var helper = container.Get<IDbAppSettingsHelper>();

            Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(helper,
                DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerUser, DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress);

            Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(helper,
               DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser, DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress);

            Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(helper,
               DbAppSettingKey.AntiAbuse_Register_MaxFrequencyPerIpAddress, DbAppSettingKey.AntiAbuse_Register_MaxFrequencyPerIpAddress);
        }

        private void Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(
            IDbAppSettingsHelper helper, DbAppSettingKey userMaxFrequencyKey, DbAppSettingKey ipAddressMaxFrequencyKey)
        {
            var userMaxFrequency = helper.GetFrequency(userMaxFrequencyKey);
            Assert.IsNotNull(userMaxFrequencyKey, string.Format("{0} has no value.", EnumsHelper.DbAppSettingKey.ToString(userMaxFrequencyKey)));

            var ipAddressMaxFrequency = helper.GetFrequency(ipAddressMaxFrequencyKey);
            Assert.IsNotNull(userMaxFrequencyKey, string.Format("{0} has no value.", EnumsHelper.DbAppSettingKey.ToString(ipAddressMaxFrequencyKey)));

            Assert.IsTrue(userMaxFrequency.Times <= ipAddressMaxFrequency.Times,
                string.Format("{0}.Times must be less or equal to {1}.Times.",
                EnumsHelper.DbAppSettingKey.ToString(userMaxFrequencyKey), EnumsHelper.DbAppSettingKey.ToString(ipAddressMaxFrequencyKey)));

            Assert.IsTrue(userMaxFrequency.Period >= ipAddressMaxFrequency.Period,
                string.Format("{0}.Period must be greater or equal to {1}.Period.",
                EnumsHelper.DbAppSettingKey.ToString(userMaxFrequencyKey), EnumsHelper.DbAppSettingKey.ToString(ipAddressMaxFrequencyKey)));
        }
    }
}
