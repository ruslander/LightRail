using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail.Core
{
    public class Oplog2 : IDisposable
    {
        private readonly string _name;
        private readonly long _segmentCapacity;

        public readonly List<ISegment> Segments;
        public HotSegment CurrentSegment;

        public Oplog2(string name, long segmentCapacity)
        {
            _name = name;
            _segmentCapacity = segmentCapacity;

            Segments = Directory
                .GetFiles(".", string.Format("{0}*.sf", _name))
                .Select(x => ColdSegment.AsReadonly(x, _name))
                .OrderBy(x => x.Position)
                .ToList();

            //Console.WriteLine("Detected [{0}] {1}",name, Segments.Count);

            if(!Segments.Any())
                RollNewSegment();
            else
            {
                var last = Segments.Last();
                var blocks = last.FetchForward().ToList();

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

        public Oplog2(string name) : this(name, 4 * Units.MEGA)
        {
        }

        public void Append(byte[] content)
        {
            try
            {
                CurrentSegment.Append(content);
            }
            catch (HotSegmentFullException)
            {
                RollNewSegment();

                CurrentSegment.Append(content);
            }
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

        public IEnumerable<Op> Forward()
        {
            long segmentOffset = 0;
            foreach (var segment in Segments)
            {
                var blockOffset = 0;
                foreach (Block block in segment.FetchForward())
                {
                    var opOffset = 0;
                    foreach (byte[] bytes in block.Forward())
                    {
                        var position = segmentOffset + blockOffset + opOffset;
                        yield return Op.ReadFrom(bytes, position);

                        opOffset = opOffset + bytes.Length;
                    }

                    blockOffset = blockOffset + block.Payload.Length;
                }

                segmentOffset = segmentOffset + _segmentCapacity;
            }
        }

        public IEnumerable<Op> Backward()
        {
            var segmentOffset = Segments.Count * _segmentCapacity;
            foreach (var segment in Segments.ToList().OrderByDescending(x => x.Position))
            {
                segmentOffset = segmentOffset - _segmentCapacity;

                var blockOffset = _segmentCapacity;
                foreach (var block in segment.FetchBackward())
                {
                    blockOffset = blockOffset - block.Payload.Length;

                    var opOffset = block.Payload.Length;
                    var records = block.Forward().Reverse();

                    foreach (var bytes in records)
                    {
                        opOffset = opOffset - bytes.Length;

                        var position = segmentOffset + blockOffset + opOffset;
                        yield return Op.ReadFrom(bytes, position);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (CurrentSegment != null)
                CurrentSegment.Burner.Dispose();
        }
    }
}