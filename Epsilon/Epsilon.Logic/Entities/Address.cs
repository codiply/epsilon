using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Address : BaseEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string UniqueAddressCode { get; set; }
        public virtual string Line1 { get; set; }
        public virtual string Line2 { get; set; }
        public virtual string Line3 { get; set; }
        public virtual string Line4 { get; set; }
        public virtual string Locality { get; set; }
        public virtual string Region { get; set; }
        public virtual string Postcode { get; set; }
        public virtual string CountryId { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }

        public string FullAddress()
        {
            var sb = new StringBuilder();
            var pieces = new List<string> { Line1, Line2, Line3, Line4, Locality, Region, Postcode };
            if (Country != null)
                pieces.Add(Country.LocalName);

            return string.Join(", ", pieces.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
