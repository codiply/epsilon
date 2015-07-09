using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Wrappers
{
    [TestFixture]
    public class SystemClockTest
    {
        IClock _clock = new SystemClock();

        [Test]
        public void OffsetNow_GivesTheCurrentDateTimeOffset()
        {
            var actualNowBefore = DateTimeOffset.Now;
            var now = _clock.OffsetNow;
            var actualNowAfter = DateTimeOffset.Now;

            Assert.IsTrue(actualNowAfter <= now && now <= actualNowAfter);
        }

        [Test]
        public void OffsetNow_GivesDateTimeOffset_WithDateTimeWithUnspecifiedKind()
        {
            var now = _clock.OffsetNow;

            var kind = now.DateTime.Kind;

            Assert.IsTrue(kind == DateTimeKind.Unspecified);
        }

        [Test]
        public void OffsetUtcNow_GivesTheCurrentDateTimeOffset()
        {
            var actualNowBefore = DateTimeOffset.UtcNow;
            var now = _clock.OffsetUtcNow;
            var actualNowAfter = DateTimeOffset.UtcNow;

            Assert.IsTrue(actualNowAfter <= now && now <= actualNowAfter);
        }

        [Test]
        public void OffsetUtcNow_GivesDateTimeWithZeroOffset()
        {
            var now = _clock.OffsetUtcNow;

            var offset = now.Offset;

            Assert.AreEqual(TimeSpan.Zero, offset);
        }
    }
}
