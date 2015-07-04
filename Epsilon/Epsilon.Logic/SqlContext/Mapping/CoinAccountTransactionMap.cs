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
    public class CoinAccountTransactionMap : EntityTypeConfiguration<CoinAccountTransaction>
    {
        public const int REFERENCE_MAX_LENGTH = 256;

        public CoinAccountTransactionMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.Reference)
                .HasMaxLength(REFERENCE_MAX_LENGTH);
            this.Property(x => x.MadeOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // Relationships
            this.HasRequired(x => x.Account)
                .WithMany(y => y.Transactions)
                .HasForeignKey(x => x.AccountId)
                .WillCascadeOnDelete(true);
            this.HasRequired(x => x.Type)
                .WithMany()
                .HasForeignKey(x => x.TypeId)
                .WillCascadeOnDelete(false);

            // Indexes
            this.Property(x => x.AccountId)
                .HasColumnAnnotation("Index", 
                    new IndexAnnotation(new IndexAttribute("IX_CoinAccountTransaction_AccountId_MadeOn", 1)));
            this.Property(x => x.MadeOn)
                .HasColumnAnnotation("Index", 
                    new IndexAnnotation(new IndexAttribute("IX_CoinAccountTransaction_AccountId_MadeOn", 2)));
        }
    }
}
