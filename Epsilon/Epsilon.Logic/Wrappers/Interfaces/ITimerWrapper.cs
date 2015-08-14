using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface ITimerWrapper
    {
        event ElapsedEventHandler Elapsed;

        bool AutoReset { get; set; }

        double IntervalMilliseconds { get; set; }

        void Start();

        void Stop();
    }
}
