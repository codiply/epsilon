using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Helpers;

namespace Epsilon.IntegrationTests.Logic.Constants
{
    public class TokenAccountTransactionTypeIdTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task ThereShouldBeATokenAccountTransactionTypeId_ForAllTokenAccountTransactionTypesInTheDatabase()
        {
            var enumTokenAccountTransactionTypeIds = EnumsHelper.TokenAccountTransactionTypeId.GetNames().ToDictionary(x => x);

            var tokenAccountTransactionTypesInDb = await DbProbe.TokenAccountTransactionTypes.ToListAsync();

            var failingTokenAccountTransactionTypes = tokenAccountTransactionTypesInDb
                .Where(c => !enumTokenAccountTransactionTypeIds.ContainsKey(c.Id))
                .ToList();

            var message = "";
            if (failingTokenAccountTransactionTypes.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingTokenAccountTransactionTypes.Count() == 1 ? " is " : " are ")
                    .Append(failingTokenAccountTransactionTypes.Count())
                    .Append(failingTokenAccountTransactionTypes.Count() == 1 ? " TokenAccountTransactionType" : " TokenAccountTransactionTypes")
                    .Append(" with missing Id in Constants.TokenAccountTransactionTypeId enumeration.");
                foreach (var c in failingTokenAccountTransactionTypes)
                {
                    sb.Append("\n").Append(c.Id).Append(" - ").Append(c.Description);
                }

                message = sb.ToString();
            }

            Assert.IsFalse(failingTokenAccountTransactionTypes.Any(), message);
        }

        [Test]
        public async Task EveryTokenAccountTransactionTypeIdShouldHaveATokenAccountTransactionTypeInTheDatabase()
        {
            var enumTokenAccountTransactionTypeIds = EnumsHelper.TokenAccountTransactionTypeId.GetNames();

            var availableTokenAccountTransactionTypesInDb = 
                await DbProbe.TokenAccountTransactionTypes.ToDictionaryAsync(x => x.Id);

            var failingTokenAccountTransactionTypeIds = enumTokenAccountTransactionTypeIds
                .Where(id => !availableTokenAccountTransactionTypesInDb.ContainsKey(id))
                .ToList();

            var message = "";
            if (failingTokenAccountTransactionTypeIds.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingTokenAccountTransactionTypeIds.Count() == 1 ? " is " : " are ")
                    .Append(failingTokenAccountTransactionTypeIds.Count())
                    .Append(" TokenAccountTransactionTypeId")
                    .Append(failingTokenAccountTransactionTypeIds.Count() == 1 ? "" : "'s")
                    .Append(" in Constants.TokenAccountTransactionTypeId enumeration with missing TokenAccountTransactionType in the database: ")
                    .Append(String.Join(", ", failingTokenAccountTransactionTypeIds))
                    .Append(".");
                message = sb.ToString();
            }

            Assert.IsFalse(failingTokenAccountTransactionTypeIds.Any(), message);
        }
    }
}
