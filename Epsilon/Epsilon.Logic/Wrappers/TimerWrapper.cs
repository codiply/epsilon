using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Epsilon.Logic.Wrappers
{
    public class TimerWrapper : ITimerWrapper
    {
        private readonly System.Timers.Timer _timer;

        public event ElapsedEventHandler Elapsed;

        public TimerWrapper()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnElapsed);
        }

        public bool AutoReset
        {
            get { return _timer.AutoReset; }
            set { _timer.AutoReset = value; }
        }

        public double IntervalMilliseconds
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (Elapsed != null)
            {
                Elapsed(this, e);
            }
        }
    }
}
