using Epsilon.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IGeoipRotatingClient
    {
        Task<GeoipClientResponse> Geoip(string ipAddress);
    }
}
