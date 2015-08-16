using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Logic.Entities
{
    public class Country
    {
        public virtual string Id { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual string LocalName { get; set; }
        public virtual string CurrencyId { get; set; }
        public virtual string MainLanguageId { get; set; }
        public virtual bool IsAvailable { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual Language MainLanguage { get; set; }

        [NotMapped]
        public virtual CountryId? IdAsEnum
        {
            get
            {
                return EnumsHelper.CountryId.Parse(Id);
            }
        }
    }
}
