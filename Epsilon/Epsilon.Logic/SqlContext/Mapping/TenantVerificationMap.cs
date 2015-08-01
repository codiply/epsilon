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
            this.Property(x => x.SecretCode)
                .IsRequired()
                .HasMaxLength(CODE_MAX_LENGTH);
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.AssignedByIpAddress)
                .IsRequired()
                .HasMaxLength(AppConstant.IP_ADDRESS_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.TenancyDetailsSubmission)
                .WithMany(y => y.TenantVerifications)
                .HasForeignKey(x => x.TenancyDetailsSubmissionId)
                .WillCascadeOnDelete(true);
            this.HasRequired(x => x.AssignedTo)
                .WithMany()
                .HasForeignKey(x => x.AssignedToId)
                .WillCascadeOnDelete(false);

            // Indexes
            this.Property(x => x.UniqueId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenantVerification_UniqueId") { IsUnique = true },
                    new IndexAttribute("IX_TenantVerification_UniqueId_AssignedToId", 1)
                }));

            this.Property(x => x.TenancyDetailsSubmissionId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenantVerification_TenancyDetailsSubmissionId"),
                    new IndexAttribute("IX_TenantVerification_TenancyDetailsSubmissionId_SecretCode", 1) { IsUnique = true }
                }));

            this.Property(x => x.SecretCode)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenantVerification_TenancyDetailsSubmissionId_SecretCode", 2) { IsUnique = true }
                }));

            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[]
                {
                    new IndexAttribute("IX_TenantVerification_CreatedOn"),
                    new IndexAttribute("IX_TenantVerification_CreatedOn_AssignedByIpAddress", 1),
                    new IndexAttribute("IX_TenantVerification_CreatedOn_AssignedToId", 1),
                    new IndexAttribute("IX_TenantVerification_CreatedOn_AssignedToId_VerifiedOn", 1)
                }));

            this.Property(x => x.AssignedToId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenantVerification_CreatedOn_AssignedToId", 2),
                    new IndexAttribute("IX_TenantVerification_UniqueId_AssignedToId", 2),
                    new IndexAttribute("IX_TenantVerification_AssignedToId_AssignedByIpAddress", 1),
                    new IndexAttribute("IX_TenantVerification_AssignedToId_VerifiedOn", 1),
                    new IndexAttribute("IX_TenantVerification_CreatedOn_AssignedToId_VerifiedOn", 2)
                }));

            this.Property(x => x.AssignedByIpAddress)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenantVerification_CreatedOn_AssignedByIpAddress", 2),
                    new IndexAttribute("IX_TenantVerification_AssignedToId_AssignedByIpAddress", 2)
                }));

            this.Property(x => x.VerifiedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TenantVerification_AssignedToId_VerifiedOn", 2),
                    new IndexAttribute("IX_TenantVerification_CreatedOn_AssignedToId_VerifiedOn", 3)
                }));
        }
    }
}
