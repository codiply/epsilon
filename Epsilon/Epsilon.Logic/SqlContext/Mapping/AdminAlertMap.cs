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
    public class AdminAlertMap : EntityTypeConfiguration<AdminAlert>
    {
        public const int KEY_MAX_LENGTH = 256;

        public AdminAlertMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.Key)
                .HasMaxLength(KEY_MAX_LENGTH)
                .IsRequired();
        }
    }
}
