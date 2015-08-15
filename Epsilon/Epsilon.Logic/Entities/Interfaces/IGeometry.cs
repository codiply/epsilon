using Epsilon.Logic.JsonModels;

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

    public static class IGeometryExtensions
    {
        public static AddressGeometryResponse ToAddressGeometryResponse(this IGeometry geometry)
        {
            if (geometry == null)
                return null;

            return new AddressGeometryResponse
            {
                latitude = geometry.Latitude,
                longitude = geometry.Longitude,
                viewportNortheastLatitude = geometry.ViewportNortheastLatitude,
                viewportNortheastLongitude = geometry.ViewportNortheastLongitude,
                viewportSouthwestLatitude = geometry.ViewportSouthwestLatitude,
                viewportSouthwestLongitude = geometry.ViewportSouthwestLongitude
            };
        }
    }
}
