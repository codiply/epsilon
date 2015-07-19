﻿using Epsilon.Logic.Entities.Interfaces;
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
    public class Address : BaseEntity, IAddress
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
        
        public virtual AddressGeometry Geometry { get; set; }
        public virtual User CreatedBy { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }

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
    }
}
