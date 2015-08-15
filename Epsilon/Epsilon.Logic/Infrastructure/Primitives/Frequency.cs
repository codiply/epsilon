using System;

namespace Epsilon.Logic.Infrastructure.Primitives
{
    public class Frequency
    {
        public int Times { get; private set; }
        public TimeSpan Period { get; set; }

        public Frequency(int times, TimeSpan period)
        {
            Times = times;
            Period = period;
        }
    }
}
