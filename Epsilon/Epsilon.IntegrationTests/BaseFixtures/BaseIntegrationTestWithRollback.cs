using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Web.App_Start;
using Moq;
using Ninject;
using NUnit.Framework;
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
            _transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            _dbProbe = new EpsilonContext();
            // I clear the cache at the beginning of each test.
            var cache = CreateContainer().Get<IAppCache>();
            cache.Clear();
        }

        [TearDown]
        public void BaseTestTearDown()
        {
            _transactionScope.Dispose();
            _transactionScope = null;
            _dbProbe.Dispose();
            _dbProbe = null;
        }

        public static async Task<User> CreateUser(IKernel container, string email, string ipAddress, bool doSetup = true)
        {
            var languageId = "en";
            var dbContext = container.Get<IEpsilonContext>();
            var newUserService = container.Get<INewUserService>();
            
            var user = new User
            {
                Email = email,
                PasswordHash = "PasswordHash",
                UserName = email
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            if (doSetup)
            {
                await newUserService.Setup(user.Id, ipAddress, languageId);
            }

            return user;
        }

        public static IKernel CreateContainer()
        {
            var kernel = new StandardKernel();
            NinjectWebCommon.RegisterServices(kernel);
            return kernel;
        }

        public static void KillDatabase(IKernel container)
        {
            var mockDbContext = new Mock<IEpsilonContext>();
            container.Rebind<IEpsilonContext>().ToConstant(mockDbContext.Object);
        }
    }
}
