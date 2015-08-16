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
