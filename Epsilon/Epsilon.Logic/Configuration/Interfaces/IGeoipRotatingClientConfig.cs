namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IGeoipRotatingClientConfig
    {
        int MaxRotations { get; }
        string ProviderRotation { get; }
    }
}
