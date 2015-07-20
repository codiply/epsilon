using GeocodeSharp.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IGeocodeClientWrapper
    {
        Task<GeocodeResponse> GeocodeAddress(string address, string region);
    }
}
