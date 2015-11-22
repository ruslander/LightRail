using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace LightRail
{
    public interface IManageSegments
    {
        List<FileSegment> GetAll();
        FileSegment AllocateSegment(string prefix, long position);
    }

    public class SegmentManager : IManageSegments
    {
        public List<FileSegment> GetAll()
        {
            return Directory.GetFiles(".", "*.sf")
                .OrderBy(x=>x)
                .Select(x=> new FileSegment(){Name = x}).ToList();
        }

        public FileSegment AllocateSegment(string prefix, long position)
        {
            var name = string.Format("{0}{1}.sf", prefix, position.ToString("D12"));

            return new FileSegment() { Name = name };
        }
    }

    public class FileSegment
    {
        public string Name { get; set; }
        public BinaryWriter Writer { get; set; }
    }

    public class Oplog
    {
        public const int Mb = 1048576;
        public string Name { get; set; }

        public List<FileSegment> Segments;
        public FileSegment CurrentSegment;

        public IManageSegments ManageSegments;

        public Oplog(string name) : this(name, new SegmentManager()){}

        public Oplog(string name, IManageSegments sm)
        {
            Name = name;

            var segments = sm.GetAll();
            
            Segments = segments;

            if(segments.Any())
                CurrentSegment = segments.LastOrDefault();
            else
            {
                CurrentSegment = sm.AllocateSegment(Name,0);
            }
        }

        public void Append(byte[] payload)
        {
            var op = new Op(payload);

            op.WriteTo(CurrentSegment.Writer); 
        }
    }

    [TestFixture]
    public class OplogSpec
    {
        [Test]
        public void ctor_loads_last_segment()
        {
            var sm = Substitute.For<IManageSegments>();
            sm.GetAll().Returns(new List<FileSegment>() { new FileSegment() { Name = "f1" }, new FileSegment() { Name = "f2" } });

            var sut = new Oplog("a", sm);

            Assert.That(sut.Segments.Count, Is.EqualTo(2));
            Assert.That(sut.CurrentSegment.Name, Is.EqualTo("f2"));
        }

        [Test]
        public void ctor_will_create_new_segemnt()
        {
            var sm = Substitute.For<IManageSegments>();
            sm.GetAll().Returns(new List<FileSegment>() { });
            sm.AllocateSegment("a", 0).Returns(new FileSegment());

            var sut = new Oplog("a", sm);

            Assert.That(sut.CurrentSegment, Is.Not.Null);
        }


        [Test]
        public void appent_uses_current_segment()
        {
            var storage = new MemoryStream();
            var fileSegment = new FileSegment() { Writer = new BinaryWriter(storage) };

            var sm = Substitute.For<IManageSegments>();
            sm.GetAll().Returns(new List<FileSegment>() { });
            sm.AllocateSegment("a", 0).Returns(fileSegment);

            var sut = new Oplog("a", sm);
            sut.Append(new byte[] {1});

            Assert.That(storage.ToArray().Length, Is.EqualTo(25));
        }
    }
}