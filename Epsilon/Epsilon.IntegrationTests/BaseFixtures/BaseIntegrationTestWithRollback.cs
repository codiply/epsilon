using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Web.App_Start;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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

        public static void SetupElmahHelper(IKernel container, Action<Exception> elmahHelperRaiseHandler)
        {
            var mockElmahHelper = new Mock<IElmahHelper>();

            mockElmahHelper.Setup(x => x.Raise(It.IsAny<Exception>())).Callback(elmahHelperRaiseHandler);

            container.Rebind<IElmahHelper>().ToConstant(mockElmahHelper.Object);
        }

        public static void SetupAdminAlertService(IKernel container, Action<string, bool> sendCallback)
        {
            var mockAdminAlertService = new Mock<IAdminAlertService>();

            mockAdminAlertService.Setup(x => x.SendAlert(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback(sendCallback);

            container.Rebind<IAdminAlertService>().ToConstant(mockAdminAlertService.Object);
        }

        public static void SetupAdminEventLogService(
            IKernel container, Action<AdminEventLogKey, Dictionary<string, object>> logCallback)
        {
            var mockAdminEventLogService = new Mock<IAdminEventLogService>();

            mockAdminEventLogService.Setup(x => x.Log(It.IsAny<AdminEventLogKey>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.FromResult(1))
                .Callback(logCallback);

            container.Rebind<IAdminEventLogService>().ToConstant(mockAdminEventLogService.Object);
        }
    }
}
