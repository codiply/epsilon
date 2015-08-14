using Epsilon.Logic.JsonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public virtual Byte[] Timestamp { get; set; }

        public virtual User User { get; set; }
        public virtual Address Address { get; set; }

        public DateTimeOffset ExpiresOn(TimeSpan expiryPeriod)
        {
            // TODO_TEST_PANOS
            return CreatedOn + expiryPeriod;
        }

        public bool CanViewInfo(DateTimeOffset now, TimeSpan expiryPeriod)
        {
            // TODO_TEST_PANOS
            return now < ExpiresOn(expiryPeriod);
        }

        public ExploredPropertyInfo ToExploredPropertyInfo(DateTimeOffset now, TimeSpan expiryPeriod)
        {
            // TODO_TEST_PANOS
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
