using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TokenRewardSchemeMap : EntityTypeConfiguration<TokenRewardScheme>
    {
        public TokenRewardSchemeMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Indexes
            this.Property(x => x.EffectiveFrom)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_TokenRewardScheme_EffectiveFrom")));
        }
    }
}
