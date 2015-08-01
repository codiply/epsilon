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

            var maxOutstandingFrequencyPerUserForNewUser = helper
                .GetFrequency(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser);
            Assert.IsNotNull(maxOutstandingFrequencyPerUserForNewUser, "MaxOutstandingFrequencyPerUserForNewUser has no value.");
            Assert.IsTrue(maxOutstandingFrequencyPerUserForNewUser.Times >= 4,
                String.Format("The MaxOutstandingFrequencyPerUserForNewUser.Times ({0}) must be greater or equal to 4 as some of the verifications might fail on the recipient side.",
                maxOutstandingFrequencyPerUserForNewUser.Times));

            var maxOutstandingFrequencyPerUser = helper
                .GetFrequency(DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUser);
            Assert.IsNotNull(maxOutstandingFrequencyPerUser, "MaxOutstandingFrequencyPerUser has no value.");
            Assert.IsTrue(maxOutstandingFrequencyPerUser.Times >= maxOutstandingFrequencyPerUserForNewUser.Times,
                String.Format("The MaxOutstandingFrequencyPerUser.Times ({0}) must be greater or equal to MaxOutstandingFrequencyPerUserForNewUser.Times ({1}).",
                maxOutstandingFrequencyPerUser.Times, maxOutstandingFrequencyPerUserForNewUser.Times));
        }

        [Test]
        public void AntiAbuse_UserMaxFrequency_vs_IpAddressMaxFrequency_Rules()
        {
            var container = CreateContainer();
            var helper = container.Get<IDbAppSettingsHelper>();

            Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(helper,
                DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerUser, 
                DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress);

            Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(helper,
               DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser, 
               DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress);

            Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(helper,
                DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUser,
                DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress);

            Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(helper,
                DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser,
                DbAppSettingKey.AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress);
        }

        [Test]
        public void MaxRetriesRules()
        {
            var container = CreateContainer();
            var helper = container.Get<IDbAppSettingsHelper>();

            var geocodeServiceOverQueryLimitMaxRetries = helper.GetInt(DbAppSettingKey.GeocodeService_OverQueryLimitMaxRetries);
            Assert.IsTrue(geocodeServiceOverQueryLimitMaxRetries.Value >= 0,
                string.Format("{0} should have non-negative value.", EnumsHelper.DbAppSettingKey.ToString(DbAppSettingKey.GeocodeService_OverQueryLimitMaxRetries)));
        }

        private void Check_UserMaxFrequency_vs_IpAddressMaxFrequency_Rule(
            IDbAppSettingsHelper helper, DbAppSettingKey userMaxFrequencyKey, DbAppSettingKey ipAddressMaxFrequencyKey, bool comparePeriods = true)
        {
            var userMaxFrequency = helper.GetFrequency(userMaxFrequencyKey);
            Assert.IsNotNull(userMaxFrequencyKey, string.Format("{0} has no value.", EnumsHelper.DbAppSettingKey.ToString(userMaxFrequencyKey)));

            var ipAddressMaxFrequency = helper.GetFrequency(ipAddressMaxFrequencyKey);
            Assert.IsNotNull(userMaxFrequencyKey, string.Format("{0} has no value.", EnumsHelper.DbAppSettingKey.ToString(ipAddressMaxFrequencyKey)));

            Assert.IsTrue(userMaxFrequency.Times <= ipAddressMaxFrequency.Times,
                string.Format("{0}.Times must be less or equal to {1}.Times.",
                EnumsHelper.DbAppSettingKey.ToString(userMaxFrequencyKey), EnumsHelper.DbAppSettingKey.ToString(ipAddressMaxFrequencyKey)));
            if (comparePeriods)
            {
                Assert.IsTrue(userMaxFrequency.Period >= ipAddressMaxFrequency.Period,
                    string.Format("{0}.Period must be greater or equal to {1}.Period.",
                    EnumsHelper.DbAppSettingKey.ToString(userMaxFrequencyKey), EnumsHelper.DbAppSettingKey.ToString(ipAddressMaxFrequencyKey)));
            }
        }
    }
}
