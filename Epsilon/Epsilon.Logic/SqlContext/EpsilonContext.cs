using Epsilon.Logic.Entities;
using Epsilon.Logic.SqlContext.Mapping;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
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

        public virtual IDbSet<Address> Addresses { get; set; }
        public virtual IDbSet<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }
        public virtual IDbSet<TenantVerification> TenantVerifications { get; set; }
        // Users DbSet is defined in IdentityDbContext (base of ApplicationDbContext).

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This needs to go before the other rules!

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new AddressMap());
            modelBuilder.Configurations.Add(new TenancyDetailsSubmissionMap());
            modelBuilder.Configurations.Add(new TenantVerificationMap());
        }
    }
}
