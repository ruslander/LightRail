using System;
using System.Linq;
using LightRail;
using NUnit.Framework;

namespace Specs
{
    [TestFixture]
    public class HotSegmentSpec : SpecificationWithFile
    {
        [Test]
        public void AppendBeforeReachingMaxCapacityTest()
        {
            var activeSegment = new HotSegment(45 * Units.MEGA)
            {
                Burner = new HotSegmentBurner(Filename, 45 * Units.MEGA, 0),
            }; 

            for (var i = 0; i < 10 * 100; i++)
            {
                var next = Guid.NewGuid().ToByteArray();

                activeSegment.Append(next);
            }

            Assert.That(activeSegment.Blocks.Count, Is.EqualTo(11));         // blocks
            Assert.That((10752 * 4 * Units.KILO) / Units.MEGA, Is.EqualTo(42)); // mb
        }

        [Test]
        public void AppendStopsAtMaxCapacityTest()
        {
            var activeSegment = new HotSegment(10 * Units.KILO)
            {
                Burner = new HotSegmentBurner(Filename, 10 * Units.KILO, 0),
            }; ;

            try
            {
                for (var i = 0; i < 10 * 100; i++)
                {
                    var next = Guid.NewGuid().ToByteArray();

                    activeSegment.Append(next);
                }
            }
            catch (HotSegmentFullException)
            {
            }

            Assert.That(activeSegment.Blocks.Count, Is.EqualTo(2));
            Assert.That(activeSegment.Blocks.Select(x => x.Payload.Length).Sum() / Units.KILO, Is.EqualTo(8));
        }
    }
}