using Epsilon.Logic.Models;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IGeoipRotatingClient
    {
        Task<GeoipClientResponse> Geoip(string ipAddress, int rotationNo = 1);
    }
}
