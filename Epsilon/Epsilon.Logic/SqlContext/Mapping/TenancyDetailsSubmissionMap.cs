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
            this.Property(x => x.UniqueId)
                .IsRequired();
            this.Property(x => x.Rent)
                .IsOptional();
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.CreatedByIpAddress)
                .IsRequired()
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
            this.Property(x => x.UniqueId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenancyDetailsSubmission_UniqueId") { IsUnique = true },
                    new IndexAttribute("IX_TenancyDetailsSubmission_UniqueId_UserId", 1)
                }));

            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[]
                {
                    new IndexAttribute("IX_TenancyDetailsSubmission_CreatedOn"),
                    new IndexAttribute("IX_TenancyDetailsSubmission_CreatedOn_CreatedByIpAddress", 1),
                    new IndexAttribute("IX_TenancyDetailsSubmission_CreatedOn_UserId", 1)
                }));

            this.Property(x => x.UserId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenancyDetailsSubmission_CreatedOn_UserId", 2),
                    new IndexAttribute("IX_TenancyDetailsSubmission_UniqueId_UserId", 2),
                    new IndexAttribute("IX_TenancyDetailsSubmission_UserId_CreatedByIpAddress",1)
                }));

            this.Property(x => x.CreatedByIpAddress)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenancyDetailsSubmission_CreatedOn_CreatedByIpAddress", 2),
                    new IndexAttribute("IX_TenancyDetailsSubmission_UserId_CreatedByIpAddress", 2)
                }));
        }
    }
}
