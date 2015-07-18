using Epsilon.Logic.Entities;
using GeocodeSharp.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IGeocodingService
    {
        Task<AddressGeometry> GeocodeAddress(string address, string region);

        Task<PostcodeGeometry> GeocodePostcode(string postcode, string region);
    }
}
