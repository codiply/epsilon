using Epsilon.Logic.Wrappers;
using NUnit.Framework;
using System;
using System.Threading;
using System.Timers;

namespace Epsilon.UnitTests.Logic.Wrappers
{
    [TestFixture]
    public class TimerWrapperTest
    {
        [Test]
        public void TriggersAfterTheSetInterval()
        {
            var intervalMilliseconds = 200.0;
            var intervalDeltaMilliseconds = 30;
            DateTime? triggeredOn = null;

            var timer = new TimerWrapper();
            timer.AutoReset = false;
            timer.IntervalMilliseconds = intervalMilliseconds;
            timer.Elapsed += new ElapsedEventHandler((source, args) => triggeredOn = DateTime.Now);

            var timerStartedOn = DateTime.Now;
            timer.Start();
            Thread.Sleep(TimeSpan.FromMilliseconds(1.2 * intervalMilliseconds));

            Assert.IsNotNull(triggeredOn, "Timer was not triggered after the interval.");
            var actualIntervalMilliseconds = (triggeredOn.Value - timerStartedOn).TotalMilliseconds;
            Assert.AreEqual(actualIntervalMilliseconds, intervalMilliseconds, intervalDeltaMilliseconds,
                "The actual interval is not close enough to the expected value.");
        }

        [Test]
        public void TriggersOnceIfAutoresetIsFalse()
        {
            var intervalMilliseconds = 100.0;
            int timesTriggered = 0;

            var timer = new TimerWrapper();
            timer.AutoReset = false;
            timer.IntervalMilliseconds = intervalMilliseconds;
            timer.Elapsed += new ElapsedEventHandler((source, args) => timesTriggered++);

            timer.Start();
            Thread.Sleep(TimeSpan.FromMilliseconds(3.0 * intervalMilliseconds));

            Assert.AreEqual(1, timesTriggered,
                "The number of times the timer triggerred is not the expecteds.");
        }

        [Test]
        public void TriggersManyTimesIfAutoresetIsTrue()
        {
            var intervalMilliseconds = 100.0;
            var expectedTimesTriggered = 3;
            int numbersTriggered = 0;

            var timer = new TimerWrapper();
            timer.AutoReset = true;
            timer.IntervalMilliseconds = intervalMilliseconds;
            timer.Elapsed += new ElapsedEventHandler((source, args) => numbersTriggered++);

            timer.Start();
            Thread.Sleep(TimeSpan.FromMilliseconds((expectedTimesTriggered + 0.5) * intervalMilliseconds));

            Assert.AreEqual(expectedTimesTriggered, numbersTriggered,
                "The number of times the timer triggerred is not the expecteds.");
        }

        [Test]
        public void DoesNotTriggerIfStopped()
        {
            var intervalMilliseconds = 100.0;
            DateTime? triggeredOn = null;

            var timer = new TimerWrapper();
            timer.AutoReset = false;
            timer.IntervalMilliseconds = intervalMilliseconds;
            timer.Elapsed += new ElapsedEventHandler((source, args) => triggeredOn = DateTime.Now);


            timer.Start();
            Thread.Sleep(TimeSpan.FromMilliseconds(0.5 * intervalMilliseconds));
            timer.Stop();
            Thread.Sleep(TimeSpan.FromMilliseconds(intervalMilliseconds));

            Assert.IsNull(triggeredOn, "The Elapsed event should not be triggered because the timer was stopped.");
        }
    }
}
