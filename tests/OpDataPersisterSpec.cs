using System;
using System.IO;
using System.Linq;
using StableStorage;
using StableStorage.Specs.Io;
using NUnit.Framework;

namespace Specs
{
    [TestFixture]
    public class OpDataPersisterSpec : SpecificationWithFile
    {
         [Test]
         public void AppendTest()
         {
             var log = new OpDataPersister(Cfg4Mb);
             log.Append(Guid.NewGuid().ToByteArray());

             Assert.That(FileExists("000000000000.sf"), Is.True);

             Assert.That(log.Segments.Count, Is.EqualTo(1));
             Assert.That(log.Segments[0].FetchForward().Count(), Is.EqualTo(1));
         }

         [Test]
         public void MultiAppendReadCountsTest()
         {
             var wr = new OpDataPersister(Cfg4K);

             for (int i = 0; i < 100; i++)
                 wr.Append(Guid.NewGuid().ToByteArray());

             wr.Dispose();

             Assert.That(FileExists("000000000000.sf"), Is.True);
             Assert.That(FileExists("000000004096.sf"), Is.True);

             var rd = new OpDataPersister(Cfg4K);
             var ops = rd.Forward().ToList();

             foreach (var op in ops)
                 new Guid(op.Payload);

             Assert.That(ops.Count(), Is.EqualTo(100));
         }

        [Test]
         public void ReopenLastSegmentTest()
         {
             var wr = new OpDataPersister(Cfg4K);

             for (int i = 0; i < 100; i++)
                 wr.Append(Guid.NewGuid().ToByteArray());

             wr.Dispose();

             var rd = new OpDataPersister(Cfg4K);
                rd.Append(Guid.NewGuid().ToByteArray());
             
             Assert.That(rd.CurrentSegment.Position, Is.EqualTo(4096));
             Assert.That(rd.Segments.Count, Is.EqualTo(wr.Segments.Count));

             var rdBlocks = rd.Segments[1].FetchForward();
             var wrBlocks = wr.Segments[1].FetchForward();

             Assert.That(rdBlocks.Count(), Is.EqualTo(wrBlocks.Count()));

             var rdRecords = rdBlocks.Select(x => x.Records().Count).Sum();
             var wrRecords = wrBlocks.Select(x => x.Records().Count).Sum();

             Assert.That(rdRecords, Is.EqualTo(wrRecords + 1));
         }

        [Test]
         public void ListForwardTest()
         {
             var wr = new OpDataPersister(Cfg4K);

             for (int i = 0; i < 100; i++)
                 wr.Append(Guid.NewGuid().ToByteArray());

             wr.Dispose();

             var rd = new OpDataPersister(Cfg4K);

             long prev = -1;

             foreach (var op in rd.Forward())
             {
                 var position = op.Position;

                 Assert.That(position, Is.GreaterThan(prev));

                 prev = position;
             }
         }

        [Test]
         public void ListBackwardTest()
         {
             var wr = new OpDataPersister(Cfg4K);

             for (int i = 0; i < 100; i++)
                 wr.Append(BitConverter.GetBytes(i));

             wr.Dispose();

             var rd = new OpDataPersister(Cfg4K);

             long prev = long.MaxValue;

             foreach (var op in rd.Backward())
             {
                 var position = op.Position;

                 Assert.That(position, Is.LessThan(prev));

                 Console.WriteLine(op.Position + " " + BitConverter.ToInt32(op.Payload, 0));

                 prev = position;
             }
         }

        [Test]
        public void DefaultSizeMultiAppendReadCountsTest()
        {
            var wr = new OpDataPersister(Cfg4Mb);

            for (int i = 0; i < 100; i++)
                wr.Append(Guid.NewGuid().ToByteArray());

            wr.Dispose();

            Assert.That(wr.Segments[0].FetchForward().Count(), Is.EqualTo(2));
            Assert.That(wr.CurrentSegment.Blocks.Count, Is.EqualTo(2));
            Assert.That(wr.CurrentSegment.Blocks.Select(x => x.Records().Count).Sum(), Is.EqualTo(100));


            var rd = new OpDataPersister(Cfg4Mb);
            var ops = rd.Forward().ToList();

            foreach (var op in ops)
                new Guid(op.Payload);

            Assert.That(rd.Segments[0].FetchForward().Count(), Is.EqualTo(2));
            Assert.That(rd.CurrentSegment.Blocks.Count, Is.EqualTo(2));
            Assert.That(rd.CurrentSegment.Blocks.Select(x => x.Records().Count).Sum(), Is.EqualTo(100));
        }

        [Test]
        public void ReopenEmptyLog()
        {
            var wr = new OpDataPersister(Cfg4K);
            wr.Dispose();

            var rd = new OpDataPersister(Cfg4K);
            var ops = rd.Forward().ToList();

            Assert.That(ops.Count, Is.EqualTo(0));
        }

        [Test]
        public void HeadEmptyLog()
        {
            var wr = new OpDataPersister(Cfg4K);
            Assert.That(wr.Head(), Is.Null);
        }

        [Test]
        public void TailEmptyLog()
        {
            var wr = new OpDataPersister(Cfg4K);
            Assert.That(wr.Tail(), Is.Null);
        }
    }
}