using Epsilon.Logic.SqlContext;
using Epsilon.Logic.TestDataPopulation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.TestDataPopulation
{
    public class TestDataPopulator : ITestDataPopulator
    {
        private readonly IEpsilonContext  _dbContext;

        public TestDataPopulator(
            IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
