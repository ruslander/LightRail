using System;
using System.Linq;
using LightRail.Core;
using NUnit.Framework;

namespace LightRail
{
    [TestFixture]
    public class SegmentSpec
    {
        [Test]
        public void AppendBeforeReachingMaxCapacityTest()
        {
            var activeSegment = new Segment(45 * Units.MEGA);

            for (var i = 0; i < 10 * 100 * 1000; i++)
            {
                var next = Guid.NewGuid().ToByteArray();

                activeSegment.Append(next);
            }

            Assert.That(activeSegment.Blocks.Count, Is.EqualTo(10752));         // blocks
            Assert.That((10752 * 4 * Units.KILO) / Units.MEGA, Is.EqualTo(42)); // mb
        }

        [Test]
        public void AppendStopsAtMaxCapacityTest()
        {
            var activeSegment = new Segment(10 * Units.KILO);

            try
            {
                for (var i = 0; i < 10 * 100; i++)
                {
                    var next = Guid.NewGuid().ToByteArray();

                    activeSegment.Append(next);
                }
            }
            catch (SegmentFullException)
            {
            }

            Assert.That(activeSegment.Blocks.Count, Is.EqualTo(2));
            Assert.That(activeSegment.Blocks.Select(x => x.Payload.Length).Sum() / Units.KILO, Is.EqualTo(8));
        }
    }
}