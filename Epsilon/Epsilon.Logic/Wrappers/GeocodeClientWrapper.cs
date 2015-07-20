using Epsilon.Logic.Wrappers.Interfaces;
using GeocodeSharp.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
