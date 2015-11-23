using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace LightRail
{
    [TestFixture]
    public class OplogSpec
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            foreach (var file in Directory.GetFiles(".", "*.sf"))
                File.Delete(file);
        }

        [Test]
        public void ctor_loads_last_segment()
        {
            var sut = new Oplog("a", new SegmentManager("a", false));

            Assert.That(sut.Segments.Count, Is.EqualTo(1));
            Assert.That(sut.CurrentSegment.Name, Is.EqualTo("a000000000000.sf"));
        }

        [Test]
        public void ctor_will_create_new_segemnt()
        {
            var sm = Substitute.For<IManageSegments>();
            sm.GetAll().Returns(new List<FileSegment>() { });
            sm.AllocateSegment(0).Returns(new FileSegment());

            var sut = new Oplog("a", sm);

            Assert.That(sut.CurrentSegment, Is.Not.Null);
        }


        [Test]
        public void append_uses_current_segment()
        {
            var storage = new MemoryStream();
            var fileSegment = new FileSegment() { Writer = new BinaryWriter(storage) };

            var sm = Substitute.For<IManageSegments>();
            sm.GetAll().Returns(new List<FileSegment>() { });
            sm.AllocateSegment(0).Returns(fileSegment);

            var sut = new Oplog("a", sm);
            sut.Append(new byte[] {1});

            Assert.That(storage.ToArray().Length, Is.EqualTo(25));
        }

        [Test]
        public void append_rolls_current_segment_when_limit_is_reached()
        {
            var sut = new Oplog("a", new SegmentManager("a", false)) { Mb = 1000 };

            for (int i = 0; i < 100; i++)
                sut.Append(new byte[] {(byte) i});

            Assert.That(sut.Segments.Count, Is.EqualTo(3));
        }

        [Test]
        public void append_persist_segment()
        {
            var sut = new Oplog("a", new SegmentManager("a"));
            sut.Append(new byte[] { 1 });

            Assert.That(File.Exists("a000000000000.sf"), Is.True);
        }

        [Test]
        public void append_rolls_at_1mb_segment_by_default()
        {
            var sut = new Oplog("b");

            for (int i = 0; i < 50000; i++)
                sut.Append(Encoding.UTF8.GetBytes("this is a filler"));

            Assert.That(File.Exists("b000000000000.sf"), Is.True);
            Assert.That(File.Exists("b000001048576.sf"), Is.True);
        }
    }
}