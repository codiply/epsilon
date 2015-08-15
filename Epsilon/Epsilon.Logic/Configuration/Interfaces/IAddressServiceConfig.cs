namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IAddressServiceConfig
    {
        bool GlobalSwitch_DisableAddAddress { get; }
        int SearchAddressResultsLimit { get; }
        int SearchPropertyResultsLimit { get; }
    }
}
