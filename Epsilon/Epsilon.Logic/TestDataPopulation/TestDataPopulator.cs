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
        private readonly IRandomFactory _randomFactory;
        private readonly IEpsilonContext  _dbContext;

        public TestDataPopulator(
            IRandomFactory randomFactory,
            IEpsilonContext dbContext)
        {
            _randomFactory = randomFactory;
            _dbContext = dbContext;
        }

        public async Task Populate(string userId)
        {
            int postCodesPerArea = 5;
            int housesPerPostcode = 5;
            int minAddressesPerHouse = 10;
            int maxAddressesPerHouse = 20;
            int seed = 2015;
            var random = _randomFactory.Create(seed);

            await GbAddressPopulator.Populate(
                random, _dbContext, userId, postCodesPerArea, housesPerPostcode, minAddressesPerHouse, maxAddressesPerHouse);
        }
    }
}
