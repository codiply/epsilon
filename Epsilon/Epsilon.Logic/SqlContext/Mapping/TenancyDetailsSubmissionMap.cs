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
    public class TenancyDetailsSubmissionMap : EntityTypeConfiguration<TenancyDetailsSubmission>
    {
        public TenancyDetailsSubmissionMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.CreatedByIpAddress)
                .HasMaxLength(AppConstant.IP_ADDRESS_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .WillCascadeOnDelete(false);

            this.HasRequired(x => x.Address)
                .WithMany(y => y.TenancyDetailsSubmissions)
                .HasForeignKey(x => x.AddressId)
                .WillCascadeOnDelete(false);

            this.HasOptional(x => x.Currency)
                .WithMany()
                .HasForeignKey(x => x.CurrencyId)
                .WillCascadeOnDelete(false);

            // Indexes
            this.Property(x => x.CreatedByIpAddress)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_CreatedByIpAddress_CreatedOn", 1)));
            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_CreatedByIpAddress_CreatedOn", 2)));

            this.Property(x => x.UserId)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_UserId_CreatedOn", 1)));
            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_UserId_CreatedOn", 2)));
        }
    }
}
