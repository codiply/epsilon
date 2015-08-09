using Epsilon.Logic.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Address : IAddress
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual string DistinctAddressCode { get; set; }
        public virtual string Line1 { get; set; }
        public virtual string Line2 { get; set; }
        public virtual string Line3 { get; set; }
        public virtual string Line4 { get; set; }
        public virtual string Locality { get; set; }
        public virtual string Region { get; set; }
        public virtual string Postcode { get; set; }
        public virtual string CountryId { get; set; }

        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual string CreatedById { get; set; }
        public virtual string CreatedByIpAddress { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual Country Country { get; set; }
        public virtual AddressGeometry Geometry { get; set; }
        public virtual PostcodeGeometry PostcodeGeometry { get; set; }
        public virtual ICollection<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }
        public virtual ICollection<PropertyInfoAccess> PropertyInfoAccesses { get; set; }

        /// <summary>
        /// NOTE: You will need to include the Country on the Address for this to work properly.
        /// </summary>
        /// <returns></returns>
        public string FullAddress()
        {
            var sb = new StringBuilder();
            sb.Append(this.FullAddressWithoutCountry());
            if (Country != null)
            {
                sb.Append(", ").Append(Country.LocalName);
            }
            return sb.ToString();
        }

        // TODO_PANOS_TEST
        public string LocalityRegionPostcode()
        {
            var pieces = new List<string> { Locality, Region, Postcode };

            return string.Join(", ", pieces.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
