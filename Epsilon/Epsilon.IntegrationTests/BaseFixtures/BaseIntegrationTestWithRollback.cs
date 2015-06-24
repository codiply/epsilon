using Epsilon.Logic.SqlContext;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Epsilon.IntegrationTests.BaseFixtures
{
    [TestFixture]
    public class BaseIntegrationTestWithRollback
    {
        private TransactionScope _transactionScope;
        private EpsilonContext _dbProbe;

        public EpsilonContext DbProbe { get { return _dbProbe; } }

        [SetUp]
        public void BaseTestSetUp()
        {
            _transactionScope = new TransactionScope();
            _dbProbe = new EpsilonContext();
        }

        [TearDown]
        public void BaseTestTearDown()
        {
            _transactionScope.Dispose();
            _transactionScope = null;
            _dbProbe = null;
        }
    }
}
