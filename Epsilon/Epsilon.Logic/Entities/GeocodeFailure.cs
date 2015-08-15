using System;

namespace Epsilon.Logic.Entities
{
    public class GeocodeFailure
    {
        public virtual long Id { get; set; }
        public virtual string Address { get; set; }
        public virtual string CountryId { get; set; }
        public virtual string QueryType { get; set; }
        public virtual string FailureType { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual string CreatedById { get; set; }
        public virtual string CreatedByIpAddress { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual Country Country { get; set; }
    }
}
