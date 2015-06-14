using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers
{
    public class SystemClock : IClock
    {
        public DateTime Now { get { return DateTime.Now; } }

        public DateTime UtcNow { get { return DateTime.UtcNow; } }

        public DateTimeOffset OffsetNow { get { return DateTimeOffset.Now; } }

        public DateTimeOffset OffsetUtcNow { get { return DateTimeOffset.UtcNow; } }
    }
}
