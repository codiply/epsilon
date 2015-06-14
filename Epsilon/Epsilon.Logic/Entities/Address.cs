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
        public virtual string Id { get; set; }
        
        public virtual string Line1 { get; set; }
        public virtual string Line2 { get; set; }
        public virtual string Line3 { get; set; }
        public virtual string CityTown { get; set; }
        public virtual string CountyStateProvince { get; set; }
        public virtual string PostcodeOrZip { get; set; }
        public virtual string CountryId { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }
    }
}
