using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IGeocodeServiceConfig
    {
        string GoogleApiServerKey { get; }
        int OverQueryLimitMaxRetries { get; }
        TimeSpan OverQueryLimitDelayBetweenRetries { get; }
    }
}
