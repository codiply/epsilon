using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface ICommonConfig
    {
        TimeSpan DefaultAppCacheSlidingExpiration { get; }
    }
}
