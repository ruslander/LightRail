using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail
{
    public class Oplog : IDisposable
    {
        private readonly string _name;
        private readonly long _segmentCapacity;

        public readonly List<ISegment> Segments;
        public HotSegment CurrentSegment;

        public Oplog(string name, long segmentCapacity)
        {
            _name = name;
            _segmentCapacity = segmentCapacity;

            Segments = Directory
                .GetFiles(".", string.Format("{0}*.sf", _name))
                .Select(x => ColdSegment.Load(x, _name))
                .OrderBy(x => x.Position)
                .ToList();

            //Console.WriteLine("Detected [{0}] {1}",name, Segments.Count);

            if(!Segments.Any())
                RollNewSegment();
            else
            {
                var last = Segments.Last();
                var blocks = last.FetchForward().ToList();

                Console.WriteLine("Last [{0}] {1}", last.Position, blocks.Count());

                last.Dispose();

                Segments.Remove(last);

                CurrentSegment = new HotSegment(_segmentCapacity, blocks)
                {
                    Burner = new HotSegmentBurner(_name, _segmentCapacity, last.Position),
                    Position = last.Position
                };

                Segments.Add(CurrentSegment);
            }
        }

        public Oplog(string name = "") : this(name, 4 * Units.MEGA)
        {
        }

        public long Append(byte[] content)
        {
            long current = 0;
            try
            {
                current = CurrentSegment.Append(content);
            }
            catch (HotSegmentFullException)
            {
                RollNewSegment();

                current = CurrentSegment.Append(content);
            }

            return current + (Segments.Count - 1) * _segmentCapacity;
        }

        private void RollNewSegment()
        {
            if (CurrentSegment != null)
                CurrentSegment.Burner.Dispose();

            var position = Segments.Count * _segmentCapacity;

            CurrentSegment = new HotSegment(_segmentCapacity)
            {
                Burner = new HotSegmentBurner(_name, _segmentCapacity, position),
                Position = position
            };

            Segments.Add(CurrentSegment);

            //Console.WriteLine("Rolling [{0}] {1}", _name, position);
        }

        public IEnumerable<Op> Forward(long position = 0, int sliceSize = int.MaxValue)
        {
            var delivered = 0;

            foreach (var segment in Segments)
            {
                foreach (Block block in segment.FetchForward())
                {
                    foreach (byte[] record in block.Forward())
                    {
                        var op = Op.ReadFrom(record);

                        if (op.Position >= position && delivered < sliceSize)
                        {
                            delivered++;
                            yield return op;
                        }
                    }
                }
            }
        }

        public IEnumerable<Op> Backward(long position = int.MaxValue, int sliceSize = int.MaxValue)
        {
            var delivered = 0;

            foreach (var segment in Segments.ToList().OrderByDescending(x => x.Position))
            {
                foreach (var block in segment.FetchBackward())
                {
                    foreach (var record in block.Forward().Reverse())
                    {
                        var op = Op.ReadFrom(record);

                        if (op.Position <= position && delivered < sliceSize)
                        {
                            delivered++;
                            yield return op;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (CurrentSegment != null)
                CurrentSegment.Burner.Dispose();
        }

        // max
        public Op Head()
        {
            var sg = Segments.ToList().OrderByDescending(x => x.Position).First();
            var block = sg.FetchBackward().First();
            var record = block.Forward().Reverse().First();

            return Op.ReadFrom(record);
        }

        // 0
        public Op Tail()
        {
            var sg = Segments.First();
            var block = sg.FetchForward().First();
            var record = block.Forward().First();

            return Op.ReadFrom(record);
        }
    }
}