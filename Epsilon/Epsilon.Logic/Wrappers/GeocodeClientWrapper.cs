using Epsilon.Logic.Wrappers.Interfaces;
using GeocodeSharp.Google;

namespace Epsilon.Logic.Wrappers
{
    public class GeocodeClientWrapper : GeocodeClient, IGeocodeClientWrapper
    {
        public GeocodeClientWrapper(string apiKey)
            :base(apiKey)
        {
        }
    }
}
