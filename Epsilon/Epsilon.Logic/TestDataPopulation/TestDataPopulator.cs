using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.TestDataPopulation.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.TestDataPopulation
{
    public class TestDataPopulator : ITestDataPopulator
    {
        private readonly IRandomWrapper _random;
        private readonly IEpsilonContext  _dbContext;

        public TestDataPopulator(
            IRandomWrapper random,
            IEpsilonContext dbContext)
        {
            _random = random;
            _dbContext = dbContext;
        }

        public async Task Populate(string userId)
        {
            int postCodesPerArea = 5;
            int housesPerPostcode = 5;
            int minAddressesPerHouse = 10;
            int maxAddressesPerHouse = 20;
            await GbAddressPopulator.Populate(
                _random, _dbContext, userId, postCodesPerArea, housesPerPostcode, minAddressesPerHouse, maxAddressesPerHouse);
        }
    }
}
