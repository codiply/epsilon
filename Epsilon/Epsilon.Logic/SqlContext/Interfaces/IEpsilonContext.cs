using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Interfaces
{
    public interface IEpsilonContext
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();

        DbSet<Address> Addresses { get; set; }
        DbSet<AdminAlert> AdminAlerts { get; set; }
        DbSet<AppSetting> AppSettings { get; set; }
        DbSet<Country> Countries { get; set; }
        DbSet<Currency> Currencies { get; set; }
        DbSet<Language> Languages { get; set; }
        DbSet<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }
        DbSet<TenantVerification> TenantVerifications { get; set; }
        // Users needs to be an IDbSet
        IDbSet<User> Users { get; set; }
        DbSet<UserPreference> UserPreferences { get; set; }
    }
}
