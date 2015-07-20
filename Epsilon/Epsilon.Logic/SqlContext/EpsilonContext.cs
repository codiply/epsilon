using Epsilon.Logic.Entities;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.SqlContext.Mapping;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext
{
    public class EpsilonContext : ApplicationDbContext, IEpsilonContext
    {
        public EpsilonContext() : base("EpsilonContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<AddressGeometry> AddressGeometries { get; set; }
        public virtual DbSet<AdminAlert> AdminAlerts { get; set; }
        public virtual DbSet<AppSetting> AppSettings { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<GeocodeFailure> GeocodeFailures { get; set; }
        public virtual DbSet<IpAddressActivity> IpAddressActivities { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<PostcodeGeometry> PostcodeGeometries { get; set; }
        public virtual DbSet<ResponseTiming> ResponseTimings { get; set; }
        public virtual DbSet<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }
        public virtual DbSet<TenantVerification> TenantVerifications { get; set; }
        public virtual DbSet<TokenAccount> TokenAccounts { get; set; }
        public virtual DbSet<TokenAccountSnapshot> TokenAccountSnapshots { get; set; }
        public virtual DbSet<TokenAccountTransaction> TokenAccountTransactions { get; set; }
        public virtual DbSet<TokenAccountTransactionType> TokenAccountTransactionTypes { get; set; }
        // Users DbSet is defined in IdentityDbContext (base of ApplicationDbContext).
        public virtual DbSet<UserPreference> UserPreferences { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This needs to go before the other rules!

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new AddressMap());
            modelBuilder.Configurations.Add(new AddressGeometryMap());
            modelBuilder.Configurations.Add(new AdminAlertMap());
            modelBuilder.Configurations.Add(new AppSettingMap());
            modelBuilder.Configurations.Add(new TokenAccountMap());
            modelBuilder.Configurations.Add(new TokenAccountSnapshotMap());
            modelBuilder.Configurations.Add(new TokenAccountTransactionMap());
            modelBuilder.Configurations.Add(new TokenAccountTransactionTypeMap());
            modelBuilder.Configurations.Add(new CountryMap());
            modelBuilder.Configurations.Add(new CurrencyMap());
            modelBuilder.Configurations.Add(new GeocodeFailureMap());
            modelBuilder.Configurations.Add(new IpAddressActivityMap());
            modelBuilder.Configurations.Add(new LanguageMap());
            modelBuilder.Configurations.Add(new PostcodeGeometryMap());
            modelBuilder.Configurations.Add(new ResponseTimingMap());
            modelBuilder.Configurations.Add(new TenancyDetailsSubmissionMap());
            modelBuilder.Configurations.Add(new TenantVerificationMap());
            modelBuilder.Configurations.Add(new UserPreferenceMap());
        }

        public override async Task<int> SaveChangesAsync()
        {
            try
            {
                return await base.SaveChangesAsync();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", 
                            validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                throw dbEx;
            }
        }
    }
}
