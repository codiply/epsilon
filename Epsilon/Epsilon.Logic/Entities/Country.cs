using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Country
    {
        public virtual string Id { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual string LocalName { get; set; }
        public virtual string CurrencyId { get; set; }
        public virtual bool IsAvailable { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual Currency Currency { get; set; }

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
