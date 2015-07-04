using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Ninject.Extensions.Factory;
using Epsilon.Logic.Entities;

namespace Epsilon.IntegrationTests.BaseFixtures
{
    [TestFixture]
    public class BaseIntegrationTestWithRollback
    {
        private TransactionScope _transactionScope;
        private EpsilonContext _dbProbe;
        private IKernel _kernel;

        public EpsilonContext DbProbe { get { return _dbProbe; } }
        public IKernel Kernel { get { return _kernel; } }

        [SetUp]
        public void BaseTestSetUp()
        {
            _transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            _dbProbe = new EpsilonContext();
            _kernel = new StandardKernel();
            RegisterServices(_kernel);
        }

        [TearDown]
        public void BaseTestTearDown()
        {
            _transactionScope.Dispose();
            _transactionScope = null;
            _dbProbe = null;
            _kernel = null;
        }

        public async Task<User> CreateUser(string email)
        {
            var languageId = "en";
            var dbContext = Kernel.Get<IEpsilonContext>();
            var newUserService = Kernel.Get<INewUserService>();
            
            var user = new User
            {
                Email = email,
                PasswordHash = "PasswordHash",
                UserName = email
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            await newUserService.Setup(user.Id, languageId);

            return user;
        }

        private static void RegisterServices(IKernel kernel)
        {
            // Constants
            kernel.Bind<IDbAppSettingDefaultValue>().To<DbAppSettingDefaultValue>().InTransientScope();
            kernel.Bind<IAppSettingsDefaultValue>().To<AppSettingsDefaultValue>().InTransientScope();

            // DbContext
            kernel.Bind<IEpsilonContext>().To<EpsilonContext>().InTransientScope();

            // Helpers
            kernel.Bind<NameValueCollection>().ToConstant(ConfigurationManager.AppSettings)
                .WhenInjectedExactlyInto<AppSettingsHelper>();
            kernel.Bind<IAppSettingsHelper>().To<AppSettingsHelper>().InTransientScope();
            kernel.Bind<ICountryVariantResourceHelper>().To<CountryVariantResourceHelper>().InSingletonScope();
            kernel.Bind<IDbAppSettingsHelper>().To<DbAppSettingsHelper>().InTransientScope();
            kernel.Bind<IIpAddressHelper>().To<IpAddressHelper>().InSingletonScope();
            kernel.Bind<IParseHelper>().To<ParseHelper>().InTransientScope();

            // Infrastructure
            kernel.Bind<IAppCache>().To<AppCache>().InTransientScope();

            // Services
            kernel.Bind<IAddressService>().To<AddressService>().InTransientScope();
            kernel.Bind<IAdminAlertService>().To<AdminAlertService>().InTransientScope();
            kernel.Bind<ICoinAccountService>().To<CoinAccountService>().InTransientScope();
            kernel.Bind<ICountryService>().To<CountryService>().InTransientScope();
            kernel.Bind<ILanguageService>().To<LanguageService>().InTransientScope();
            kernel.Bind<INewUserService>().To<NewUserService>().InTransientScope();
            kernel.Bind<ISmtpService>().To<SmtpService>().InTransientScope();
            kernel.Bind<ITenancyDetailsSubmissionService>().To<TenancyDetailsSubmissionService>().InTransientScope();
            kernel.Bind<IUserPreferenceService>().To<UserPreferenceService>().InTransientScope();
            kernel.Bind<IUserCoinService>().To<UserCoinService>().InTransientScope();
            
            // Wrappers
            kernel.Bind<ICacheWrapper>().To<HttpRuntimeCache>().InTransientScope();
            kernel.Bind<IClock>().To<SystemClock>().InTransientScope();
            kernel.Bind<IRandomWrapper>().To<RandomWrapper>().InTransientScope();
            kernel.Bind<ISmtpClientWrapper>().To<SmtpClientWrapper>().InTransientScope();
            kernel.Bind<ISmtpClientWrapperFactory>().ToFactory();
        }
    }
}
