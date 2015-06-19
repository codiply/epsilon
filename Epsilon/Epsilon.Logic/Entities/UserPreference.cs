﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class UserPreference : BaseEntity
    {
        [Key, ForeignKey("User")]
        public virtual string Id { get; set; }
        public virtual string LanguageId { get; set; }

        public virtual User User { get; set; }
        public virtual Language Language { get; set; } 
    }
}
