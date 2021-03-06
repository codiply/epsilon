[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Epsilon.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Epsilon.Web.App_Start.NinjectWebCommon), "Stop")]

namespace Epsilon.Web.App_Start
{
    using Epsilon.Logic.Configuration;
    using Epsilon.Logic.Configuration.Interfaces;
    using Epsilon.Logic.Constants;
    using Epsilon.Logic.Constants.Interfaces;
    using Epsilon.Logic.Entities;
    using Epsilon.Logic.Helpers;
    using Epsilon.Logic.Helpers.Interfaces;
    using Epsilon.Logic.Infrastructure;
    using Epsilon.Logic.Infrastructure.Interfaces;
    using Epsilon.Logic.Services;
    using Epsilon.Logic.Services.Interfaces;
    using Epsilon.Logic.SqlContext;
    using Epsilon.Logic.SqlContext.Interfaces;
    using Epsilon.Logic.TestDataPopulation;
    using Epsilon.Logic.TestDataPopulation.Interfaces;
    using Epsilon.Logic.Wrappers;
    using Epsilon.Logic.Wrappers.Interfaces;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Extensions.Factory;
    using Ninject.Web.Common;
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.Entity;
    using System.Web;

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

            // Configuration
            kernel.Bind<IAddressServiceConfig>().To<AddressServiceConfig>().InRequestScope();
            kernel.Bind<IAdminAlertServiceConfig>().To<AdminAlertServiceConfig>().InRequestScope();
            kernel.Bind<IAntiAbuseServiceConfig>().To<AntiAbuseServiceConfig>().InRequestScope();
            kernel.Bind<IAppCacheConfig>().To<AppCacheConfig>().InRequestScope();
            kernel.Bind<ICommonConfig>().To<CommonConfig>().InRequestScope();
            kernel.Bind<IGeocodeServiceConfig>().To<GeocodeServiceConfig>().InRequestScope();
            kernel.Bind<IGeoipRotatingClientConfig>().To<GeoipRotatingClientConfig>().InRequestScope();
            kernel.Bind<IGeoipClientConfig>().To<GeoipClientConfig>().InRequestScope();
            kernel.Bind<IGeoipInfoServiceConfig>().To<GeoipInfoServiceConfig>().InRequestScope();
            kernel.Bind<IOutgoingVerificationServiceConfig>().To<OutgoingVerificationServiceConfig>().InRequestScope();
            kernel.Bind<IPropertyInfoAccessServiceConfig>().To<PropertInfoAccessServiceConfig>().InRequestScope();
            kernel.Bind<ISmtpServiceConfig>().To<SmtpServiceConfig>().InRequestScope();
            kernel.Bind<ITenancyDetailsSubmissionServiceConfig>().To<TenancyDetailsSubmissionServiceConfig>().InRequestScope();
            kernel.Bind<ITokenAccountServiceConfig>().To<TokenAccountServiceConfig>().InRequestScope();
            kernel.Bind<IUserAccountMaintenanceServiceConfig>().To<UserAccountMaintenanceServiceConfig>().InRequestScope();
            kernel.Bind<IUserTokenServiceConfig>().To<UserTokenServiceConfig>().InRequestScope();

            // Constants
            kernel.Bind<IDbAppSettingDefaultValue>().To<DbAppSettingDefaultValue>().InSingletonScope();
            kernel.Bind<IAppSettingsDefaultValue>().To<AppSettingsDefaultValue>().InSingletonScope();
            kernel.Bind<ICountryAddressFieldMetadata>().To<CountryAddressFieldMetadata>().InRequestScope();

            // DbContext
            kernel.Bind<IEpsilonContext>().To<EpsilonContext>().InRequestScope();

            // Helpers
            kernel.Bind<IAddressCleansingHelper>().To<AddressCleansingHelper>().InSingletonScope();
            kernel.Bind<IAppCacheHelper>().To<AppCacheHelper>().InSingletonScope();
            kernel.Bind<NameValueCollection>().ToConstant(ConfigurationManager.AppSettings)
                .WhenInjectedExactlyInto<AppSettingsHelper>();
            kernel.Bind<IAppSettingsHelper>().To<AppSettingsHelper>().InSingletonScope();
            kernel.Bind<ICountryVariantResourceHelper>().To<CountryVariantResourceHelper>().InSingletonScope();
            kernel.Bind<ICsvHelper>().To<CsvHelper>().InSingletonScope();
            kernel.Bind<IDbAppSettingsHelper>().To<DbAppSettingsHelper>().InRequestScope();
            kernel.Bind<IElmahHelper>().To<ElmahHelper>().InSingletonScope();
            kernel.Bind<IIpAddressHelper>().To<IpAddressHelper>().InSingletonScope();
            kernel.Bind<IParseHelper>().To<ParseHelper>().InSingletonScope();
            kernel.Bind<ITextResourceHelper>().To<TextResourceHelper>().InSingletonScope();
            kernel.Bind<ITokenRewardMetadataHelper>().To<TokenRewardMetadataHelper>().InSingletonScope();

            // Infrastructure
            kernel.Bind<IAppCache>().To<AppCache>().InSingletonScope();

            // Services
            kernel.Bind<IAddressService>().To<AddressService>().InRequestScope();
            kernel.Bind<IAddressVerificationService>().To<AddressVerificationService>().InRequestScope();
            kernel.Bind<IAdminAlertService>().To<AdminAlertService>().InRequestScope();
            kernel.Bind<IAdminEventLogService>().To<AdminEventLogService>().InRequestScope();
            kernel.Bind<IAntiAbuseService>().To<AntiAbuseService>().InRequestScope();
            kernel.Bind<ICountryService>().To<CountryService>().InRequestScope();
            kernel.Bind<ICurrencyService>().To<CurrencyService>().InRequestScope();
            kernel.Bind<IGeocodeService>().To<GeocodeService>().InRequestScope();
            kernel.Bind<IGeoipInfoService>().To<GeoipInfoService>().InRequestScope();
            kernel.Bind<IIpAddressActivityService>().To<IpAddressActivityService>().InRequestScope();
            kernel.Bind<ILanguageService>().To<LanguageService>().InRequestScope();
            kernel.Bind<INewUserService>().To<NewUserService>().InRequestScope();
            kernel.Bind<IOutgoingVerificationService>().To<OutgoingVerificationService>().InRequestScope();
            kernel.Bind<IPropertyInfoAccessService>().To<PropertyInfoAccessService>().InRequestScope();
            kernel.Bind<IResponseTimingService>().To<ResponseTimingService>().InRequestScope();
            kernel.Bind<ISmtpService>().To<SmtpService>().InRequestScope();
            kernel.Bind<ITenancyDetailsSubmissionService>().To<TenancyDetailsSubmissionService>().InRequestScope();
            kernel.Bind<ITokenAccountService>().To<TokenAccountService>().InRequestScope();
            kernel.Bind<ITokenRewardService>().To<TokenRewardService>().InRequestScope();
            kernel.Bind<IUserAccountMaintenanceService>().To<UserAccountMaintenanceService>().InRequestScope();
            kernel.Bind<IUserInterfaceCustomisationService>().To<UserInterfaceCustomisationService>().InRequestScope();
            kernel.Bind<IUserPreferenceService>().To<UserPreferenceService>().InRequestScope();
            kernel.Bind<IUserResidenceService>().To<UserResidenceService>().InRequestScope();
            kernel.Bind<IUserTokenService>().To<UserTokenService>().InRequestScope();

            // TestDataPopulation
            kernel.Bind<ITestDataPopulator>().To<TestDataPopulator>().InRequestScope();

            // Wrappers
            kernel.Bind<ICacheWrapper>().To<HttpRuntimeCache>().InSingletonScope();
            kernel.Bind<IClock>().To<SystemClock>().InSingletonScope();
            kernel.Bind<IGeocodeClientFactory>().ToFactory();
            kernel.Bind<IGeocodeClientWrapper>().To<GeocodeClientWrapper>().InTransientScope();
            kernel.Bind<IGeoipClient>().To<GeoipClient>().InRequestScope();
            kernel.Bind<IGeoipClientFactory>().ToFactory();
            kernel.Bind<IGeoipRotatingClient>().To<GeoipRotatingClient>().InRequestScope();
            kernel.Bind<IRandomFactory>().ToFactory();
            kernel.Bind<IRandomWrapper>().To<RandomWrapper>().InTransientScope();
            kernel.Bind<ISmtpClientWrapper>().To<SmtpClientWrapper>().InTransientScope();
            kernel.Bind<ISmtpClientWrapperFactory>().ToFactory();
            kernel.Bind<ITimerFactory>().ToFactory();
            kernel.Bind<ITimerWrapper>().To<TimerWrapper>().InTransientScope();
            kernel.Bind<IWebClientFactory>().ToFactory();
            kernel.Bind<IWebClientWrapper>().To<WebClientWrapper>().InTransientScope();
        }        
    }
}
