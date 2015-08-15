using GeocodeSharp.Google;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IGeocodeClientWrapper
    {
        Task<GeocodeResponse> GeocodeAddress(string address, string region);
    }
}
