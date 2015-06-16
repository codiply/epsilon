using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TenantVerificationMap : EntityTypeConfiguration<TenantVerification>
    {
        public TenantVerificationMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.Code)
                .HasMaxLength(16);

            // Relationships
            this.HasRequired(x => x.TenancyDetailsSubmission)
                .WithMany(y => y.TenantVerifications)
                .HasForeignKey(x => x.TenancyDetailsSubmissionId);
        }
    }
}
