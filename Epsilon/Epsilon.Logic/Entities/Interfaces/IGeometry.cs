using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities.Interfaces
{
    public interface IGeometry
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
        double ViewportNortheastLatitude { get; set; }
        double ViewportNortheastLongitude { get; set; }
        double ViewportSouthwestLatitude { get; set; }
        double ViewportSouthwestLongitude { get; set; }
    }
}
