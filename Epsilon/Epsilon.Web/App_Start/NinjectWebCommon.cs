[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Epsilon.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Epsilon.Web.App_Start.NinjectWebCommon), "Stop")]

namespace Epsilon.Web.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Extensions.Factory;
    using Epsilon.Logic.SqlContext.Interfaces;
    using Epsilon.Logic.Services.Interfaces;
    using Epsilon.Logic.Services;
    using Epsilon.Logic.Wrappers.Interfaces;
    using Epsilon.Logic.Wrappers;
    using Epsilon.Logic.Infrastructure.Interfaces;
    using Epsilon.Logic.Infrastructure;
    using Epsilon.Logic.Helpers.Interfaces;
    using Epsilon.Logic.Helpers;
    using System.Collections.Specialized;
    using System.Configuration;
    using Epsilon.Logic.TestDataPopulation.Interfaces;
    using Epsilon.Logic.TestDataPopulation;
    using Epsilon.Logic.SqlContext;
    using Epsilon.Logic.Constants.Interfaces;
    using Epsilon.Logic.Constants;
    using Microsoft.Owin.Security;
    using Microsoft.AspNet.Identity;
    using Epsilon.Logic.Entities;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ApplicationDbContext>().To<ApplicationDbContext>().InRequestScope();
            kernel.Bind<ApplicationSignInManager>().To<ApplicationSignInManager>().InRequestScope();
            kernel.Bind<ApplicationUserManager>().To<ApplicationUserManager>().InRequestScope();
            kernel.Bind<IAuthenticationManager>().ToMethod(c => HttpContext.Current.GetOwinContext().Authentication);
            kernel.Bind<DbContext>().To<ApplicationDbContext>().WhenInjectedExactlyInto<UserStore<User>>().InRequestScope();
            kernel.Bind<IUserStore<User>>().To<UserStore<User>>().InRequestScope();
            
            // Constants
            kernel.Bind<IDbAppSettingDefaultValue>().To<DbAppSettingDefaultValue>().InSingletonScope();
            kernel.Bind<IAppSettingsDefaultValue>().To<AppSettingsDefaultValue>().InSingletonScope();
            kernel.Bind<ICountryAddressFieldMetadata>().To<CountryAddressFieldMetadata>().InRequestScope();

            // DbContext
            kernel.Bind<IEpsilonContext>().To<EpsilonContext>().InRequestScope();

            // Helpers
            kernel.Bind<IAddressCleansingHelper>().To<AddressCleansingHelper>().InSingletonScope();
            kernel.Bind<NameValueCollection>().ToConstant(ConfigurationManager.AppSettings)
                .WhenInjectedExactlyInto<AppSettingsHelper>();
            kernel.Bind<IAppSettingsHelper>().To<AppSettingsHelper>().InSingletonScope();
            kernel.Bind<ICountryVariantResourceHelper>().To<CountryVariantResourceHelper>().InSingletonScope();
            kernel.Bind<IDbAppSettingsHelper>().To<DbAppSettingsHelper>().InRequestScope();
            kernel.Bind<IIpAddressHelper>().To<IpAddressHelper>().InSingletonScope();
            kernel.Bind<IParseHelper>().To<ParseHelper>().InSingletonScope();

            // Infrastructure
            kernel.Bind<IAppCache>().To<AppCache>().InSingletonScope();

            // Services
            kernel.Bind<IAddressService>().To<AddressService>().InRequestScope();
            kernel.Bind<IAddressVerificationService>().To<AddressVerificationService>().InRequestScope();
            kernel.Bind<IAdminAlertService>().To<AdminAlertService>().InRequestScope();
            kernel.Bind<IAntiAbuseService>().To<AntiAbuseService>().InRequestScope();
            kernel.Bind<ICoinAccountService>().To<CoinAccountService>().InRequestScope();
            kernel.Bind<ICountryService>().To<CountryService>().InRequestScope();
            kernel.Bind<IIpAddressActivityService>().To<IpAddressActivityService>().InRequestScope();
            kernel.Bind<ILanguageService>().To<LanguageService>().InRequestScope();
            kernel.Bind<INewUserService>().To<NewUserService>().InRequestScope();
            kernel.Bind<ISmtpService>().To<SmtpService>().InRequestScope();
            kernel.Bind<ITenancyDetailsSubmissionService>().To<TenancyDetailsSubmissionService>().InRequestScope();
            kernel.Bind<IUserPreferenceService>().To<UserPreferenceService>().InRequestScope();
            kernel.Bind<IUserCoinService>().To<UserCoinService>().InRequestScope();

            // TestDataPopulation
            kernel.Bind<ITestDataPopulator>().To<TestDataPopulator>().InRequestScope();

            // Wrappers
            kernel.Bind<ICacheWrapper>().To<HttpRuntimeCache>().InSingletonScope();
            kernel.Bind<IClock>().To<SystemClock>().InSingletonScope();
            kernel.Bind<IRandomWrapper>().To<RandomWrapper>().InTransientScope();
            kernel.Bind<ISmtpClientWrapper>().To<SmtpClientWrapper>().InTransientScope();
            kernel.Bind<ISmtpClientWrapperFactory>().ToFactory();
        }        
    }
}
