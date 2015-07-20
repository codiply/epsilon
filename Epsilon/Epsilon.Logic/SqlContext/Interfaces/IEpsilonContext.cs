using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Interfaces
{
    public interface IEpsilonContext
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        DbSet<Address> Addresses { get; set; }
        DbSet<AddressGeometry> AddressGeometries { get; set; }
        DbSet<AdminAlert> AdminAlerts { get; set; }
        DbSet<AppSetting> AppSettings { get; set; }
        DbSet<Country> Countries { get; set; }
        DbSet<Currency> Currencies { get; set; }
        DbSet<GeocodeFailure> GeocodeFailures { get; set; }
        DbSet<IpAddressActivity> IpAddressActivities { get; set; }
        DbSet<Language> Languages { get; set; }
        DbSet<PostcodeGeometry> PostcodeGeometries { get; set; }
        DbSet<ResponseTiming> ResponseTimings { get; set; }
        DbSet<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }
        DbSet<TenantVerification> TenantVerifications { get; set; }
        DbSet<TokenAccount> TokenAccounts { get; set; }
        DbSet<TokenAccountSnapshot> TokenAccountSnapshots { get; set; }
        DbSet<TokenAccountTransaction> TokenAccountTransactions { get; set; }
        DbSet<TokenAccountTransactionType> TokenAccountTransactionTypes { get; set; }
        // Users needs to be an IDbSet
        IDbSet<User> Users { get; set; }
        DbSet<UserPreference> UserPreferences { get; set; }
    }
}
