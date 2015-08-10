using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class GeoipInfo
    {
        public string IpAddress { get; set; }

        public string CountryCode { get; set; }
        public string ContinentCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTimeOffset RecordedOn { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public CountryId? CountryCodeAsEnum()
        {
            return EnumsHelper.CountryId.Parse(CountryCode);
        }
    }
}
