namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IGeocodeClientFactory
    {
        IGeocodeClientWrapper Create(string apiKey);
    }
}
