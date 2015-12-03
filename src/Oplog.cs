using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail
{
    public class Oplog : IDisposable
    {
        private readonly string _name;
        private readonly long _quota;

        public readonly List<ISegment> Segments;
        public HotSegment CurrentSegment;

        public Oplog(string name, long quota)
        {
            _name = name;
            _quota = quota;

            Segments = Directory
                .GetFiles(".", string.Format("{0}*.sf", _name))
                .Select(x => ColdSegment.Load(x, _name))
                .OrderBy(x => x.Position)
                .ToList();

            if(!Segments.Any())
                RollNewSegment();
            else
            {
                var last = Segments.Last();
                var blocks = last.FetchForward().ToList();

                Console.WriteLine("Last [{0}] {1}", last.Position, blocks.Count());

                last.Dispose();

                Segments.Remove(last);

                var burner = new HotSegmentBurner(_name, _quota, last.Position);

                if (blocks.Count == 0)
                    CurrentSegment = new HotSegment(_quota) {Burner = burner, Position = last.Position};
                else
                    CurrentSegment = new HotSegment(_quota, blocks){ Burner = burner, Position = last.Position };

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

            return current + (Segments.Count - 1) * _quota;
        }

        private void RollNewSegment()
        {
            if (CurrentSegment != null)
                CurrentSegment.Burner.Dispose();

            var position = Segments.Count * _quota;

            CurrentSegment = new HotSegment(_quota)
            {
                Burner = new HotSegmentBurner(_name, _quota, position),
                Position = position
            };

            Segments.Add(CurrentSegment);
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

        public Op Head()
        {
            var sg = Segments.ToList().OrderByDescending(x => x.Position).First();
            var block = sg.FetchBackward().First();
            var record = block.Forward().Reverse().FirstOrDefault();

            if (record == null)
                return null;

            return Op.ReadFrom(record);
        }

        public Op Tail()
        {
            var sg = Segments.First();
            var block = sg.FetchForward().First();
            var record = block.Forward().FirstOrDefault();

            if (record == null)
                return null;

            return Op.ReadFrom(record);
        }

        public void Dispose()
        {
            if (CurrentSegment != null)
                CurrentSegment.Burner.Dispose();
        }
    }
}