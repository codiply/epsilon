﻿using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

            // Relationships
            this.HasRequired(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);

            this.HasRequired(x => x.Address)
                .WithMany(y => y.TenancyDetailsSubmissions)
                .HasForeignKey(x => x.AddressId);

            this.HasOptional(x => x.Currency)
                .WithMany()
                .HasForeignKey(x => x.CurrencyId);
        }
    }
}