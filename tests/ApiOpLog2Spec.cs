using System;
using System.Collections.Generic;
using LightRail;
using NUnit.Framework;

namespace Specs
{
    class ApiOpLog2
    {
        public List<int> Items = new List<int>();

        public void Append(int i)
        {
            Items.Add(i);
        }

        public IEnumerable<int> Forward(int position = 0, int sliceSize = int.MaxValue)
        {
            var endAt = sliceSize == int.MaxValue ? Items.Count : position + sliceSize;

            for (int i = position; i < endAt; i++)
            {
                yield return Items[i];
            }
        }

        public IEnumerable<int> Backward(int position = int.MaxValue, int sliceSize = int.MaxValue)
        {
            var startWith = position == int.MaxValue ? Items.Count - 1 : position;
            var endAt = sliceSize == int.MaxValue ? 0 : startWith - sliceSize + 1;

            for (int i = startWith; i >= endAt; i--)
            {
                yield return Items[i];
            }
        }
    }

    [TestFixture]
    public class ApiOpLog2Spec : SpecificationWithFile
    {
        [Test]
        public void Append()
        {
            var log = new Oplog(Filename);

            log.Append(BitConverter.GetBytes(1));

            Assert.That(log.CurrentSegment.RecordsCount(), Is.EqualTo(1));
        }

        [Test]
        public void Forward()
        {
            var log = new Oplog(Filename);

            log.Append(BitConverter.GetBytes(1));
            log.Append(BitConverter.GetBytes(2));
            log.Append(BitConverter.GetBytes(3));
            log.Append(BitConverter.GetBytes(4));
            log.Append(BitConverter.GetBytes(5));

            var iter = log.Forward().GetEnumerator();
            
            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(1));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(2));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(3));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(4));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(5));
        }

        [Test]
        public void Backward()
        {
            var log = new Oplog(Filename);

            log.Append(BitConverter.GetBytes(1));
            log.Append(BitConverter.GetBytes(2));
            log.Append(BitConverter.GetBytes(3));
            log.Append(BitConverter.GetBytes(4));
            log.Append(BitConverter.GetBytes(5));

            var iter = log.Backward().GetEnumerator(); ;

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(5));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(4));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(3));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(2));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(1));
        }

        [Test]
        public void Forward_a_slice()
        {
            var log = new Oplog(Filename);

            log.Append(BitConverter.GetBytes(1));
            log.Append(BitConverter.GetBytes(2));
            var idx = log.Append(BitConverter.GetBytes(3));
            log.Append(BitConverter.GetBytes(4));
            log.Append(BitConverter.GetBytes(5));

            var items = new List<int>();

            foreach (var v in log.Forward(idx, 2))
                items.Add(BitConverter.ToInt32(v.Payload, 0));

            Assert.That(items[0], Is.EqualTo(3));
            Assert.That(items[1], Is.EqualTo(4));

            Assert.That(items.Count, Is.EqualTo(2));
        }

        [Test]
        public void Forward_from_position()
        {
            var log = new Oplog(Filename);
            log.Append(BitConverter.GetBytes(14));
            log.Append(BitConverter.GetBytes(2));
            var idx = log.Append(BitConverter.GetBytes(34));
            log.Append(BitConverter.GetBytes(234));
            log.Append(BitConverter.GetBytes(455));

            var iter = log.Forward(idx).GetEnumerator(); ;

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(34));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(234));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(455));

            Assert.That(iter.MoveNext(), Is.False);
        }

        [Test]
        public void Backward_from_position()
        {
            var log = new Oplog(Filename);
            log.Append(BitConverter.GetBytes(14));
            log.Append(BitConverter.GetBytes(2));
            var idx = log.Append(BitConverter.GetBytes(34));
            log.Append(BitConverter.GetBytes(234));
            log.Append(BitConverter.GetBytes(455));

            var iter = log.Backward(idx).GetEnumerator(); ;

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(34));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(2));

            iter.MoveNext();
            Assert.That(BitConverter.ToInt32(iter.Current.Payload, 0), Is.EqualTo(14));

            Assert.That(iter.MoveNext(), Is.False);
        }

        [Test,Ignore]
        public void Backward_a_slice()
        {
            var log = new ApiOpLog2() { Items = new List<int>() { 1, 2, 3, 4, 5 } };

            var items = new List<int>();

            foreach (var v in log.Backward(int.MaxValue, 2))
                items.Add(v);

            Assert.That(items[0], Is.EqualTo(5));
            Assert.That(items[1], Is.EqualTo(4));

            Assert.That(items.Count, Is.EqualTo(2));
        }
    }
}