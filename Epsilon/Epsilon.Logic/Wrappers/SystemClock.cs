using Epsilon.Logic.Wrappers.Interfaces;
using System;

namespace Epsilon.Logic.Wrappers
{
    public class SystemClock : IClock
    {
        public DateTimeOffset OffsetNow { get { return DateTimeOffset.Now; } }
        public DateTimeOffset OffsetUtcNow { get { return DateTimeOffset.UtcNow; } }
    }
}
