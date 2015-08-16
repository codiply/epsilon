using System;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Entities
{
    public class Language
    {
        public virtual string Id { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual string LocalName { get; set; }
        public virtual string CultureCode { get; set; }
        public virtual bool IsAvailable { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }
    }
}