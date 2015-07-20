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
    public class TokenAccountSnapshotMap : EntityTypeConfiguration<TokenAccountSnapshot>
    {
        public TokenAccountSnapshotMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.MadeOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.Balance)
                .HasPrecision(AppConstant.TOKEN_AMOUNT_PRECISION, AppConstant.TOKEN_AMOUNT_SCALE);

            // Relationships
            this.HasRequired(x => x.Account)
                .WithMany(y => y.Snapshots)
                .HasForeignKey(x => x.AccountId)
                .WillCascadeOnDelete(true);

            // Indexes
            this.Property(x => x.AccountId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_TokenAccountSnapshot_AccountId_IsFinalised_MadeOn", 1)));
            this.Property(x => x.IsFinalised)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_TokenAccountSnapshot_AccountId_IsFinalised_MadeOn", 2)));
            this.Property(x => x.MadeOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_TokenAccountSnapshot_AccountId_IsFinalised_MadeOn", 3)));
        }
    }
}
