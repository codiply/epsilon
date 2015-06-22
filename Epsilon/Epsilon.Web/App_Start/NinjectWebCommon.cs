[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Epsilon.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Epsilon.Web.App_Start.NinjectWebCommon), "Stop")]

namespace Epsilon.Web.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
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
        private static void RegisterServices(IKernel kernel)
        {
            // DbContext
            kernel.Bind<IEpsilonContext>().To<EpsilonContext>().InRequestScope();
            kernel.Bind<IDbContextWithAppSettings>().To<EpsilonContext>().InRequestScope();

            // Helpers
            kernel.Bind<NameValueCollection>().ToConstant(ConfigurationManager.AppSettings)
                .WhenInjectedExactlyInto<AppSettingsHelper>();
            kernel.Bind<IAppSettingsHelper>().To<AppSettingsHelper>().InSingletonScope();
            kernel.Bind<IParseHelper>().To<ParseHelper>().InSingletonScope();

            // Infrastructure
            kernel.Bind<IAppCache>().To<AppCache>().InSingletonScope();

            // Services
            kernel.Bind<IAddressService>().To<AddressService>().InRequestScope();
            kernel.Bind<ICountryService>().To<CountryService>().InRequestScope();
            kernel.Bind<ILanguageService>().To<LanguageService>().InRequestScope();
            kernel.Bind<ITenancyDetailsSubmissionService>().To<TenancyDetailsSubmissionService>().InRequestScope();
            kernel.Bind<IUserPreferenceService>().To<UserPreferenceService>().InRequestScope();

            // Wrappers
            kernel.Bind<ICacheWrapper>().To<HttpRuntimeCache>().InSingletonScope();
            kernel.Bind<IClock>().To<SystemClock>().InSingletonScope();
            kernel.Bind<IRandomWrapper>().To<RandomWrapper>().InTransientScope();

            // TestDataPopulation
            kernel.Bind<ITestDataPopulator>().To<TestDataPopulator>().InRequestScope();
        }        
    }
}
