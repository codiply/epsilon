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
    public class ResponseTimingMap : EntityTypeConfiguration<ResponseTiming>
    {
        public const int ACTION_MAX_LENGTH = 128;
        public const int CONTROLLER_MAX_LENGTH = 128;

        public ResponseTimingMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.MeasuredOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.ActionName)
                .IsRequired()
                .HasMaxLength(ACTION_MAX_LENGTH);
            this.Property(x => x.ControllerName)
                .IsRequired()
                .HasMaxLength(CONTROLLER_MAX_LENGTH);
        }
    }
}
