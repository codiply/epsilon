using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Constants.Enums
{
    public class TokenRewardKeyTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task ThereShouldBeATokenRewardKey_ForAllTokenRewardTypesInTheDatabase()
        {
            var enumTokenRewardKeys = EnumsHelper.TokenRewardKey.GetNames().ToDictionary(x => x);

            var tokenRewardTypesInDb = await DbProbe.TokenRewardTypes.ToListAsync();

            var failingTokenRewardTypes = tokenRewardTypesInDb
                .Where(x => !enumTokenRewardKeys.ContainsKey(x.Key))
                .ToList();

            var message = "";
            if (failingTokenRewardTypes.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingTokenRewardTypes.Count() == 1 ? " is " : " are ")
                    .Append(failingTokenRewardTypes.Count())
                    .Append(failingTokenRewardTypes.Count() == 1 ? " TokenRewardType" : " TokenRewardTypes")
                    .Append(" with missing Id in Constants.Enums.TokenRewardKey enumeration.");
                foreach (var c in failingTokenRewardTypes)
                {
                    sb.Append("\n").Append(c.Key).Append(" - ").Append(c.Description);
                }

                message = sb.ToString();
            }

            Assert.IsFalse(failingTokenRewardTypes.Any(), message);
        }

        [Test]
        public async Task EveryTokenRewardKeyShouldHaveATokenRewardTypeInTheDatabase()
        {
            var enumTokenRewardKeys = EnumsHelper.TokenRewardKey.GetNames();

            var allTokenRewardTypesInDb = 
                await DbProbe.TokenRewardTypes.ToDictionaryAsync(x => x.Key);

            var failingTokenRewardKeys = enumTokenRewardKeys
                .Where(key => !allTokenRewardTypesInDb.ContainsKey(key))
                .ToList();

            var message = "";
            if (failingTokenRewardKeys.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingTokenRewardKeys.Count() == 1 ? " is " : " are ")
                    .Append(failingTokenRewardKeys.Count())
                    .Append(" TokenRewardKey")
                    .Append(failingTokenRewardKeys.Count() == 1 ? "" : "'s")
                    .Append(" in Constants.Enums.TokenRewardKey enumeration with missing TokenRewardType in the database: ")
                    .Append(String.Join(", ", failingTokenRewardKeys))
                    .Append(".");
                message = sb.ToString();
            }

            Assert.IsFalse(failingTokenRewardKeys.Any(), message);
        }
    }
}
