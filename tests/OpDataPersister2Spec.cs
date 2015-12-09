using System;
using System.Collections.Generic;
using LightRail;
using NUnit.Framework;

namespace Specs
{
    [TestFixture]
    public class OpDataPersister2Spec : SpecificationWithFile
    {
        [Test]
        public void Append()
        {
            var log = new OpDataPersister(Cfg4Mb);

            log.Append(ToBytes(1));

            Assert.That(log.CurrentSegment.RecordsCount(), Is.EqualTo(1));
        }

        public byte[] ToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public int ToInt(byte[] value)
        {
            return BitConverter.ToInt32(value, 0);
        }

        [Test]
        public void Forward()
        {
            var log = new OpDataPersister(Cfg4Mb);

            log.Append(ToBytes(1));
            log.Append(ToBytes(2));
            log.Append(ToBytes(3));
            log.Append(ToBytes(4));
            log.Append(ToBytes(5));

            var iter = log.Forward().GetEnumerator();
            
            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(1));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(2));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(3));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(4));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(5));
        }

        [Test]
        public void Backward()
        {
            var log = new OpDataPersister(Cfg4Mb);

            log.Append(ToBytes(1));
            log.Append(ToBytes(2));
            log.Append(ToBytes(3));
            log.Append(ToBytes(4));
            log.Append(ToBytes(5));

            var iter = log.Backward().GetEnumerator(); ;

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(5));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(4));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(3));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(2));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(1));
        }

        [Test]
        public void Forward_a_slice()
        {
            var log = new OpDataPersister(Cfg4Mb);

            log.Append(ToBytes(1));
            log.Append(ToBytes(2));
            var idx = log.Append(ToBytes(3));
            log.Append(ToBytes(4));
            log.Append(ToBytes(5));

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
            var log = new OpDataPersister(Cfg4Mb);
            log.Append(ToBytes(14));
            log.Append(ToBytes(2));
            var idx = log.Append(ToBytes(34));
            log.Append(ToBytes(234));
            log.Append(ToBytes(455));

            var iter = log.Forward(idx).GetEnumerator(); ;

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(34));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(234));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(455));

            Assert.That(iter.MoveNext(), Is.False);
        }

        [Test]
        public void Backward_from_position()
        {
            var log = new OpDataPersister(Cfg4Mb);
            log.Append(ToBytes(14));
            log.Append(ToBytes(2));
            var idx = log.Append(ToBytes(34));
            log.Append(ToBytes(234));
            log.Append(ToBytes(455));

            var iter = log.Backward(idx).GetEnumerator(); ;

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(34));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(2));

            iter.MoveNext();
            Assert.That(ToInt(iter.Current.Payload), Is.EqualTo(14));

            Assert.That(iter.MoveNext(), Is.False);
        }

        [Test]
        public void Backward_a_slice()
        {
            var log = new OpDataPersister(Cfg4Mb);

            log.Append(ToBytes(1));
            log.Append(ToBytes(2));
            log.Append(ToBytes(3));
            log.Append(ToBytes(4));
            log.Append(ToBytes(5));

            var items = new List<int>();

            foreach (var v in log.Backward(int.MaxValue, 2))
                items.Add(BitConverter.ToInt32(v.Payload, 0));

            Assert.That(items[0], Is.EqualTo(5));
            Assert.That(items[1], Is.EqualTo(4));

            Assert.That(items.Count, Is.EqualTo(2));
        }

        [Test]
        public void Head()
        {
            var log = new OpDataPersister(Cfg4Mb);

            log.Append(ToBytes(1));
            log.Append(ToBytes(2));
            log.Append(ToBytes(3));
            log.Append(ToBytes(4));
            log.Append(ToBytes(5));

            var v = log.Head();

            Assert.That(BitConverter.ToInt32(v.Payload, 0), Is.EqualTo(5));
        }

        [Test]
        public void Tail()
        {
            var log = new OpDataPersister(Cfg4Mb);

            log.Append(ToBytes(1));
            log.Append(ToBytes(2));
            log.Append(ToBytes(3));
            log.Append(ToBytes(4));
            log.Append(ToBytes(5));

            var v = log.Tail();

            Assert.That(BitConverter.ToInt32(v.Payload, 0), Is.EqualTo(1));
        }
    }
}