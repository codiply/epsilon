using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Ninject;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;
using Epsilon.Logic.Helpers;
using System.Collections.Generic;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class TokenRewardServiceTest : BaseIntegrationTestWithRollback
    {
        #region GetAllTokenRewardMetadata

        [Test]
        public void GetAllTokenRewardMetadata_Works()
        {
            var container = CreateContainer();
            var service = container.Get<ITokenRewardService>();

            var allMetadata = service.GetAllTokenRewardMetadata();

            foreach (var key in EnumsHelper.TokenRewardKey.GetNames())
            {
                var metadata = allMetadata.typeMetadata.SingleOrDefault(x => x.key.Equals(key));
                Assert.IsNotNull(metadata,
                    string.Format("Metadata not found for key '{0}'.", key));
                Assert.IsNotNullOrEmpty(metadata.displayName,
                    string.Format("No displayName found for key '{0}'.", key));
                Assert.IsNotNullOrEmpty(metadata.description,
                    string.Format("No description found for key '{0}'.", key));
            }
        }

        #endregion

        #region GetCurrentScheme

        [Test]
        public async Task GetCurrentScheme_PicksTheRightSchemeAndCachesIt()
        {
            var helperContainer = CreateContainer();
            var clock = helperContainer.Get<IClock>();

            var random = new RandomWrapper(2015);

            var pastRewardScheme = await CreateRandomTokenRewardScheme(
                random, helperContainer, clock.OffsetNow - TimeSpan.FromDays(1));
            var expectedCurrentRewardScheme = await CreateRandomTokenRewardScheme(
                random, helperContainer, clock.OffsetNow);
            var futureRewardScheme = await CreateRandomTokenRewardScheme(
                random, helperContainer, clock.OffsetNow + TimeSpan.FromDays(1));

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<ITokenRewardService>();

            var actualCurrentScheme = service.GetCurrentScheme();

            Assert.AreEqual(expectedCurrentRewardScheme.Id, actualCurrentScheme.Id,
                "The Id of the current scheme is not the expected.");

            foreach (var keyEnum in EnumsHelper.TokenRewardKey.GetValues())
            {
                var key = EnumsHelper.TokenRewardKey.ToString(keyEnum);
                var expectedReward = expectedCurrentRewardScheme.Rewards.SingleOrDefault(x => x.TypeKey.Equals(key));
                var actualRewardFromScheme = actualCurrentScheme.Rewards.SingleOrDefault(x => x.TypeKey.Equals(key));
                var actualRewardFromService = service.GetCurrentReward(keyEnum);


                Assert.That(expectedReward.Value, Is.EqualTo(actualRewardFromScheme.Value).Within(AppConstant.TOKEN_REWARD_DELTA),
                    string.Format("The reward value of type '{0}' on the current scheme is not the expected.", key));
                Assert.That(expectedReward.Value, Is.EqualTo(actualRewardFromService.Value).Within(AppConstant.TOKEN_REWARD_DELTA),
                    string.Format("The reward value of type '{0}' obtained from GetCurrentReward is not the expected.", key));
            }

            KillDatabase(containerUnderTest);
            var serviceWithoutDatabase = containerUnderTest.Get<ITokenRewardService>();

            var actualCurrentSchemeCached = serviceWithoutDatabase.GetCurrentScheme();

            Assert.AreEqual(expectedCurrentRewardScheme.Id, actualCurrentSchemeCached.Id,
                "The Id of the current scheme is not the expected.");
        }

        [Test]
        public async Task GetCurrentScheme_CachesForTheRightAmountOfTime()
        {
            var currentSchemeExpiresIn = TimeSpan.FromSeconds(0.2);
            var helperContainer = CreateContainer();
            var clock = helperContainer.Get<IClock>();

            var random = new RandomWrapper(2015);

            var pastRewardScheme = await CreateRandomTokenRewardScheme(
                random, helperContainer, clock.OffsetNow - TimeSpan.FromDays(1));
            var expectedCurrentRewardScheme = await CreateRandomTokenRewardScheme(
                random, helperContainer, clock.OffsetNow);
            var futureRewardScheme = await CreateRandomTokenRewardScheme(
                random, helperContainer, clock.OffsetNow + currentSchemeExpiresIn);

            var containerUnderTest1 = CreateContainer();
            var service1 = containerUnderTest1.Get<ITokenRewardService>();

            var actualCurrentSchemeBefore = service1.GetCurrentScheme();

            Assert.AreEqual(expectedCurrentRewardScheme.Id, actualCurrentSchemeBefore.Id,
                "The Id of the current scheme before expiry is not the expected.");

            KillDatabase(containerUnderTest1);
            var service1WithoutDatabase = containerUnderTest1.Get<ITokenRewardService>();

            var actualCurrentSchemeCachedBefore = service1WithoutDatabase.GetCurrentScheme();
            Assert.AreEqual(expectedCurrentRewardScheme.Id, actualCurrentSchemeCachedBefore.Id,
                "The Id of the cached current scheme before expiry is not the expected.");

            await Task.Delay(currentSchemeExpiresIn);

            var containerUnderTest2 = CreateContainer();
            var service2 = containerUnderTest2.Get<ITokenRewardService>();

            var actualCurrentSchemeAfter = service2.GetCurrentScheme();

            Assert.AreEqual(futureRewardScheme.Id, actualCurrentSchemeAfter.Id,
                "The Id of the current scheme after expiry is not the expected.");
        }

        #endregion



        #region GetTokenRewardsSummary

        [Test]
        public async Task GetTokenRewardsSummary_Test()
        {
            var helperContainer = CreateContainer();
            var clock = helperContainer.Get<IClock>();

            var random = new RandomWrapper(2015);

            var currentScheme = await CreateRandomTokenRewardScheme(
                random, helperContainer, clock.OffsetNow);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<ITokenRewardService>();

            var summary = service.GetTokenRewardsSummary();

            foreach (var keyEnum in EnumsHelper.TokenRewardKey.GetValues())
            {
                var key = EnumsHelper.TokenRewardKey.ToString(keyEnum);
                var metadata = summary.typeMetadata.SingleOrDefault(x => x.key.Equals(key));
                Assert.IsNotNull(metadata,
                    string.Format("Metadata not found for key '{0}'.", key));
                Assert.IsNotNullOrEmpty(metadata.displayName,
                    string.Format("No displayName found for key '{0}'.", key));
                Assert.IsNotNullOrEmpty(metadata.description,
                    string.Format("No description found for key '{0}'.", key));

                var valueInEarn = summary.earnTypeValues.SingleOrDefault(x => x.key.Equals(key));
                var valueInSpend = summary.spendTypeValues.SingleOrDefault(x => x.key.Equals(key));

                if (keyEnum.EarnOrSpend() == TokenRewardKeyType.Earn)
                {
                    Assert.IsNotNull(valueInEarn,
                        string.Format("Value was not found in earnTypeValues for key '{0}'.", key));
                    Assert.IsNull(valueInSpend,
                        string.Format("Value should not be found in spendTypeValues for key '{0}'.", key));

                    var actualValue = valueInEarn.value;
                    var expectedValue = currentScheme.Rewards.Single(x => x.TypeKey.Equals(key)).Value;
                    Assert.That(actualValue, Is.EqualTo(expectedValue).Within(AppConstant.TOKEN_REWARD_DELTA),
                        string.Format("Value for reward with key '{0}' is not the expected.", key));
                }
                else
                {
                    Assert.IsNull(valueInEarn,
                        string.Format("Value should not found in earnTypeValues for key '{0}'.", key));
                    Assert.IsNotNull(valueInSpend,
                        string.Format("Value was not be found in spendTypeValues for key '{0}'.", key));

                    var actualValue = valueInSpend.value;
                    var expectedValue = currentScheme.Rewards.Single(x => x.TypeKey.Equals(key)).Value;
                    Assert.That(actualValue, Is.EqualTo(expectedValue).Within(AppConstant.TOKEN_REWARD_DELTA),
                        string.Format("Value for reward with key '{0}' is not the expected.", key));
                }

            }
        }

        #endregion

        #region Reference Data Tests

        [Test]
        public async Task RewardsHaveSignDependingOnTheirType()
        {
            var allRewards = await DbProbe.TokenRewards.ToListAsync();

            foreach (var reward in allRewards)
            {
                if (reward.TypeKey.StartsWith(AppConstant.TOKEN_REWARD_KEY_EARN))
                {
                    Assert.IsTrue(reward.Value >= 0M,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' should have non-negative value because it starts with '{1}'.",
                            reward.SchemeId, reward.TypeKey, AppConstant.TOKEN_REWARD_KEY_EARN));
                }
                else if (reward.TypeKey.StartsWith(AppConstant.TOKEN_REWARD_KEY_SPEND))
                {
                    Assert.IsTrue(reward.Value <= 0M,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' should have non-positive value because it starts with '{2}'.",
                            reward.SchemeId, reward.TypeKey, AppConstant.TOKEN_REWARD_KEY_SPEND));
                } else
                {
                    Assert.IsTrue(false,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' does not start with 'Earn' or 'Spend'.",
                            reward.SchemeId, reward.TypeKey));
                }
            }
        }

        #endregion

        #region Private Helper Functions

        public async Task<TokenRewardScheme> CreateRandomTokenRewardScheme(
            IRandomWrapper random, IKernel container, DateTimeOffset effectiveFrom)
        {
            var dbContext = container.Get<IEpsilonContext>();
            var highestId = await dbContext.TokenRewardSchemes
                .OrderByDescending(x => x.Id).Select(x => x.Id).FirstAsync();

            var tokenRewardScheme = new TokenRewardScheme
            {
                Id = highestId + 1,
                EffectiveFrom = effectiveFrom,
                Rewards = new List<TokenReward>()
            };

            dbContext.TokenRewardSchemes.Add(tokenRewardScheme);

            foreach (var key in EnumsHelper.TokenRewardKey.GetNames())
            {
                tokenRewardScheme.Rewards.Add(new TokenReward
                {
                    SchemeId = tokenRewardScheme.Id,
                    TypeKey = key,
                    Value = (decimal)(10.0 * random.NextDouble() - 20.0)
                });
            }

            await dbContext.SaveChangesAsync();
            return tokenRewardScheme;
        }
        #endregion
    }
}
