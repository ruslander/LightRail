using System;
using System.Collections.Generic;
using System.Text;
using LightRail.Core;
using NUnit.Framework;

namespace LightRail
{
    [TestFixture]
    public class BlockSpec
    {
        const int BLOCK_SIZE = 4096;

        [Test]
        public void BlockNoPositionInBlockTest()
        {
            var globalPosition = 6025;

            var blockNo = globalPosition/BLOCK_SIZE;
            var blockPosition = globalPosition % BLOCK_SIZE;

            if (blockPosition != 0)
                blockNo++;

            Assert.That(blockNo, Is.EqualTo(2));
            Assert.That(blockPosition, Is.EqualTo(1929));
        }

        [Test]
        public void EmptyBlockTest()
        {
            var block = new Block(new byte[] {});
            Assert.That(block.Records().Count, Is.EqualTo(0));
        }

        [Test]
        public void Parse_3_Records()
        {
            var layout = new byte[50];
            var idx = layout.Length - 4;

            var first = BitConverter.GetBytes(1111);
            Array.Copy(first, 0, layout, idx, 4);

            idx = idx - 4;

            var second = BitConverter.GetBytes(2222);
            Array.Copy(second, 0, layout, idx, 4);

            idx = idx - 4;

            var third = BitConverter.GetBytes(3333);
            Array.Copy(third, 0, layout, idx, 4);


            var block = new Block(layout);
            var records = block.Records();


            Assert.That(records.Count, Is.EqualTo(3));

            Assert.That(records[0], Is.EqualTo(1111));
            Assert.That(records[1], Is.EqualTo(2222));
            Assert.That(records[2], Is.EqualTo(3333));
        }
        
        [Test]
        public void Format_Payload_and_Size()
        {
            var layout = new byte[50];
            var block = new Block(layout);

            block.Append(BitConverter.GetBytes(1111));
            block.Append(Encoding.UTF8.GetBytes("hello this is very cool"));

            Assert.That(BitConverter.ToInt32(layout, 0), Is.EqualTo(1111));

            Assert.That(BitConverter.ToInt32(layout, 46), Is.EqualTo(4));
            Assert.That(BitConverter.ToInt32(layout, 42), Is.EqualTo(23));
        }

        [Test]
        public void Size_tracking()
        {
            var layout = new byte[50];
            var block = new Block(layout);

            block.Append(BitConverter.GetBytes(1111));
            block.Append(BitConverter.GetBytes(1111));

            Assert.That(BitConverter.ToInt32(layout, 42), Is.EqualTo(4));
            Assert.That(BitConverter.ToInt32(layout, 46), Is.EqualTo(4));
        }

        [Test,ExpectedException(typeof(BlockFullException))]
        public void BlockFullException_when_full()
        {
            var layout = new byte[10];
            var block = new Block(layout);

            block.Append(BitConverter.GetBytes(1111));
            block.Append(BitConverter.GetBytes(7777));
        }

        [Test]
        public void Block_end_to_end()
        {
            var appends = new List<Guid>();

            var block = Block.New();

            for (var i = 0; i < 200; i++)
            {
                var g = Guid.NewGuid();

                block.Append(g.ToByteArray());
                appends.Add(g);
            }

            var app = 0;

            foreach (var record in block.Forward())
            {
                var g = new Guid(record);
                Assert.That(g, Is.EqualTo(appends[app]));

                app++;
            }
        }

        [Test]
        public void Forward()
        {
            var block = Block.New();
            block.Append(BitConverter.GetBytes(1));
            block.Append(BitConverter.GetBytes(2));
            block.Append(BitConverter.GetBytes(3));
            block.Append(BitConverter.GetBytes(4));
            block.Append(BitConverter.GetBytes(5));

            var iter = block.Forward().GetEnumerator();

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current,0), Is.EqualTo(1));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current, 0), Is.EqualTo(2));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current, 0), Is.EqualTo(3));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current, 0), Is.EqualTo(4));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current, 0), Is.EqualTo(5));
        }
    }
}