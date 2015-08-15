using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Models;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IGeoipClient
    {
        Task<GeoipClientResponse> Geoip(GeoipProviderName providerName, string ipAddress);
    }
}
