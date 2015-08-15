using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TokenAccountTransactionMap : EntityTypeConfiguration<TokenAccountTransaction>
    {
        public const int REFERENCE_MAX_LENGTH = 256;

        public TokenAccountTransactionMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.UniqueId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            this.Property(x => x.ExternalReference)
                .IsOptional()
                .HasMaxLength(REFERENCE_MAX_LENGTH);
            this.Property(x => x.InternalReference)
                .IsOptional();

            this.Property(x => x.MadeOn)
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.Amount)
                .HasPrecision(AppConstant.TOKEN_AMOUNT_PRECISION, AppConstant.TOKEN_AMOUNT_SCALE);

            // Relationships
            this.HasRequired(x => x.Account)
                .WithMany(y => y.Transactions)
                .HasForeignKey(x => x.AccountId)
                .WillCascadeOnDelete(true);
            this.HasRequired(x => x.RewardType)
                .WithMany()
                .HasForeignKey(x => x.RewardTypeKey)
                .WillCascadeOnDelete(false);

            // Indexes
            this.Property(x => x.UniqueId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {
                    new IndexAttribute("IX_TokenAccountTransaction_UniqueId") { IsUnique = true }
                }));
            this.Property(x => x.MadeOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[]
                {
                    new IndexAttribute("IX_TokenAccountTransaction_MadeOn_AccountId", 1),
                    new IndexAttribute("IX_TokenAccountTransaction_AccountId_MadeOn", 2)
                }));
            this.Property(x => x.AccountId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[]
                {
                    new IndexAttribute("IX_TokenAccountTransaction_MadeOn_AccountId", 2),
                    new IndexAttribute("IX_TokenAccountTransaction_AccountId_MadeOn", 1)
                }));
        }
    }
}
