using Epsilon.Logic.JsonModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Entities
{
    public class PropertyInfoAccess
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual string UserId { get; set; }
        public virtual long AddressId { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual string CreatedByIpAddress { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }

        public virtual User User { get; set; }
        public virtual Address Address { get; set; }

        public DateTimeOffset ExpiresOn(TimeSpan expiryPeriod)
        {
            return CreatedOn + expiryPeriod;
        }

        public bool CanViewInfo(DateTimeOffset now, TimeSpan expiryPeriod)
        {
            return now < ExpiresOn(expiryPeriod);
        }

        public ExploredPropertyInfo ToExploredPropertyInfo(DateTimeOffset now, TimeSpan expiryPeriod)
        {
            return new ExploredPropertyInfo
            {
                accessUniqueId = UniqueId,
                displayAddress = Address.FullAddress(),
                expiresOn = CreatedOn + expiryPeriod,
                canViewInfo = CanViewInfo(now, expiryPeriod)
            };
        }
    }
}
