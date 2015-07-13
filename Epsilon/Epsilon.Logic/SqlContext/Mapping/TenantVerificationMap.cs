using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TenantVerificationMap : EntityTypeConfiguration<TenantVerification>
    {
        public const int CODE_MAX_LENGTH = 16;

        public TenantVerificationMap()
        {
        
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.UniqueId)
                .IsRequired();
            this.Property(x => x.Code)
                .HasMaxLength(CODE_MAX_LENGTH);
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.CreatedByIpAddress)
                .HasMaxLength(AppConstant.IP_ADDRESS_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.TenancyDetailsSubmission)
                .WithMany(y => y.TenantVerifications)
                .HasForeignKey(x => x.TenancyDetailsSubmissionId)
                .WillCascadeOnDelete(true);

            // Indexes
            this.Property(x => x.UniqueId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_TenantVerification_UniqueId") { IsUnique = true }));
        }
    }
}
