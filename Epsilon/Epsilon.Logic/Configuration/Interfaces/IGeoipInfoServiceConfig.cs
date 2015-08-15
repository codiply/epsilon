using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IGeoipInfoServiceConfig
    {
        TimeSpan ExpiryPeriod { get; }
    }
}
