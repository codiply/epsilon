using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Constants
{
    public class CoinAccountTransactionTypeIdTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public void ThereShouldBeACoinAccountTransactionTypeId_ForAllCoinAccountTransactionTypesInTheDatabase()
        {
            var enumCoinAccountTransactionTypeIds = Enum.GetNames(typeof(CoinAccountTransactionTypeId)).ToDictionary(x => x);

            var coinAccountTransactionTypesInDb = DbProbe.CoinAccountTransactionTypes.ToList();

            var failingCoinAccountTransactionTypes = coinAccountTransactionTypesInDb
                .Where(c => !enumCoinAccountTransactionTypeIds.ContainsKey(c.Id))
                .ToList();

            var message = "";
            if (failingCoinAccountTransactionTypes.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingCoinAccountTransactionTypes.Count() == 1 ? " is " : " are ")
                    .Append(failingCoinAccountTransactionTypes.Count())
                    .Append(failingCoinAccountTransactionTypes.Count() == 1 ? " CoinAccountTransactionType" : " CoinAccountTransactionTypes")
                    .Append(" with missing Id in Constants.CoinAccountTransactionTypeId enumeration.");
                foreach (var c in failingCoinAccountTransactionTypes)
                {
                    sb.Append("\n").Append(c.Id).Append(" - ").Append(c.Description);
                }

                message = sb.ToString();
            }

            Assert.IsFalse(failingCoinAccountTransactionTypes.Any(), message);
        }

        [Test]
        public void EveryCoinAccountTransactionTypeIdShouldHaveACoinAccountTransactionTypeInTheDatabase()
        {
            var enumCoinAccountTransactionTypeIds = Enum.GetNames(typeof(CoinAccountTransactionTypeId));

            var availableCoinAccountTransactionTypesInDb = 
                DbProbe.CoinAccountTransactionTypes.ToDictionary(x => x.Id);

            var failingCoinAccountTransactionTypeIds = enumCoinAccountTransactionTypeIds
                .Where(id => !availableCoinAccountTransactionTypesInDb.ContainsKey(id))
                .ToList();

            var message = "";
            if (failingCoinAccountTransactionTypeIds.Any())
            {
                var sb = new StringBuilder();
                sb.Append("There")
                    .Append(failingCoinAccountTransactionTypeIds.Count() == 1 ? " is " : " are ")
                    .Append(failingCoinAccountTransactionTypeIds.Count())
                    .Append(" CoinAccountTransactionTypeId")
                    .Append(failingCoinAccountTransactionTypeIds.Count() == 1 ? "" : "'s")
                    .Append(" in Constants.CoinAccountTransactionTypeId enumeration with missing CoinAccountTransactionType in the database: ")
                    .Append(String.Join(", ", failingCoinAccountTransactionTypeIds))
                    .Append(".");
                message = sb.ToString();
            }

            Assert.IsFalse(failingCoinAccountTransactionTypeIds.Any(), message);
        }
    }
}
