using System;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IClock
    {
        DateTimeOffset OffsetNow { get; }
        DateTimeOffset OffsetUtcNow { get; }
    }
}
