﻿using Epsilon.Logic.Entities;
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
    public class CoinAccountSnapshotMap : EntityTypeConfiguration<CoinAccountSnapshot>
    {
        public CoinAccountSnapshotMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // Relationships
            this.HasRequired(x => x.Account)
                .WithMany(y => y.Snapshots)
                .HasForeignKey(x => x.AccountId)
                .WillCascadeOnDelete(true);

            // Indexes
            this.Property(x => x.AccountId)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("AccountId_CreatedOn", 1)));
            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("AccountId_CreatedOn", 2)));
        }
    }
}
