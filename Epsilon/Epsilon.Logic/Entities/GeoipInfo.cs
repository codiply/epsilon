using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Entities
{
    public class GeoipInfo
    {
        public string IpAddress { get; set; }

        public string CountryCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string GeoipProviderName { get; set; }
        public DateTimeOffset RecordedOn { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }

        public CountryId? CountryCodeAsEnum()
        {
            return EnumsHelper.CountryId.Parse(CountryCode);
        }
    }
}
