using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class AddressGeometryResponse
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double viewportNortheastLatitude { get; set; }
        public double viewportNortheastLongitude { get; set; }
        public double viewportSouthwestLatitude { get; set; }
        public double viewportSouthwestLongitude { get; set; }
    }
}
