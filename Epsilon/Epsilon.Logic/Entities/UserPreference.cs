using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Logic.Entities
{
    public class UserPreference
    {
        [Key, ForeignKey("User")]
        public virtual string Id { get; set; }
        public virtual string LanguageId { get; set; }

        public virtual DateTimeOffset UpdatedOn { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }

        public virtual User User { get; set; }
        public virtual Language Language { get; set; } 
    }
}
