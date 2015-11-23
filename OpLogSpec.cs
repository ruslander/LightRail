using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace LightRail
{
    public interface IManageSegments
    {
        List<FileSegment> GetAll();
        FileSegment AllocateSegment(long position);
    }

    public class SegmentManager : IManageSegments
    {
        private readonly string _name;
        private readonly bool _persistent;

        public SegmentManager(string name, bool persistent = true)
        {
            _name = name;
            _persistent = persistent;
        }

        public List<FileSegment> GetAll()
        {
            if (!_persistent)
                return new List<FileSegment>();

            return Directory.GetFiles(".", string.Format("{0}*.sf", _name))
                .OrderBy(x=>x)
                .Select(x => new FileSegment() { Name = Path.GetFileName(x) }).ToList();
        }

        public FileSegment AllocateSegment(long position)
        {
            var name = string.Format("{0}{1}.sf", _name, position.ToString("D12"));

            Stream backStream;

            if (_persistent)
            {
                backStream = new FileStream(name, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                //backStream.SetLength(Size);
            }
            else
                backStream = new MemoryStream();

            return new FileSegment()
            {
                Name = name,
                Writer = new BinaryWriter(backStream)
            };
        }
    }

    public class FileSegment
    {
        public string Name { get; set; }
        public BinaryWriter Writer { get; set; }
    }

    public class Oplog
    {
        public int Mb = 1048576;
        public string Name { get; set; }

        public List<FileSegment> Segments;
        public FileSegment CurrentSegment;

        public IManageSegments ManageSegments;

        public Oplog(string name) : this(name, new SegmentManager(name)) { }

        public Oplog(string name, IManageSegments sm)
        {
            Name = name;
            ManageSegments = sm;

            var segments = sm.GetAll();
            
            Segments = segments;
            
            if (segments.Any())
            {
                CurrentSegment = segments.LastOrDefault();
            }
            else
            {
                var newSegment = sm.AllocateSegment(0);

                CurrentSegment = newSegment;
                Segments.Add(newSegment);
            }
        }

        public void Append(byte[] payload)
        {
            var op = new Op(payload);

            var length = CurrentSegment.Writer.BaseStream.Length;

            if (length + op.Length > Mb)
            {
                var next = Segments.Count*Mb;

                var newSegment = ManageSegments.AllocateSegment(next);

                CurrentSegment = newSegment;
                Segments.Add(newSegment);
            }

            op.WriteTo(CurrentSegment.Writer);
        }
    }

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