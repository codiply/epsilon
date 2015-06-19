using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class UserPreferenceMap : EntityTypeConfiguration<UserPreference>
    {
        public UserPreferenceMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Relationships
            this.HasOptional(x => x.Language)
                .WithMany()
                .HasForeignKey(x => x.LanguageId);
            this.HasRequired(x => x.User)
                .WithOptional(x => x.Preference);
        }
    }
}
