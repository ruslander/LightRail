using System;
using System.IO;
using System.Linq;
using LightRail.Core;
using NUnit.Framework;

namespace LightRail.Specs.Io
{
    [TestFixture]
    public class Oplog2Spec
    {
         [TestFixtureSetUp]
         public void SetUp()
         {
             foreach (var file in Directory.GetFiles(".", "*.sf"))
                 File.Delete(file);
         }

         [Test]
         public void AppendTest()
         {
             var log = new Oplog2("o");
             log.Append(Guid.NewGuid().ToByteArray());

             Assert.That(File.Exists("o000000000000.sf"), Is.True);

             Assert.That(log.Segments.Count, Is.EqualTo(1));
             Assert.That(log.Segments[0].Fetch().Count(), Is.EqualTo(1));
         }

         [Test]
         public void MultiAppendReadCountsTest()
         {
             var wr = new Oplog2("e", 4 * Units.KILO);

             for (int i = 0; i < 100; i++)
                 wr.Append(Guid.NewGuid().ToByteArray());

             wr.Dispose();

             Assert.That(File.Exists("e000000000000.sf"), Is.True);
             Assert.That(File.Exists("e000000004096.sf"), Is.True);

             var rd = new Oplog2("e", 100 * Units.KILO);
             var ops = rd.Forward().ToList();

             foreach (var op in ops)
                 new Guid(op.Payload);

             Assert.That(ops.Count(), Is.EqualTo(100));
         }

         [Test]
         public void ReopenLastSegmentTest()
         {
             var wr = new Oplog2("b", 4 * Units.KILO);

             for (int i = 0; i < 100; i++)
                 wr.Append(Guid.NewGuid().ToByteArray());

             wr.Dispose();

             var rd = new Oplog2("b", 4 * Units.KILO);
                rd.Append(Guid.NewGuid().ToByteArray());
             
             Assert.That(rd.CurrentSegment.Position, Is.EqualTo(4096));
             Assert.That(rd.Segments.Count, Is.EqualTo(wr.Segments.Count));

             var rdBlocks = rd.Segments[1].Fetch();
             var wrBlocks = wr.Segments[1].Fetch();

             Assert.That(rdBlocks.Count(), Is.EqualTo(wrBlocks.Count()));

             var rdRecords = rdBlocks.Select(x => x.Records().Count).Sum();
             var wrRecords = wrBlocks.Select(x => x.Records().Count).Sum();

             Assert.That(rdRecords, Is.EqualTo(wrRecords + 1));
         }

         [Test]
         public void ListOpPositionsTest()
         {
             var wr = new Oplog2("b", 4 * Units.KILO);

             for (int i = 0; i < 100; i++)
                 wr.Append(Guid.NewGuid().ToByteArray());

             wr.Dispose();

             var rd = new Oplog2("b", 4 * Units.KILO);

             foreach (var op in rd.Forward())
                 Console.WriteLine(op.Position);
         }
    }
}