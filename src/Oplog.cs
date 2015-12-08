using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail
{
    public class OplogConfig
    {
        public string Name;
        public string BasePath;
        public long Quota;
        public bool Fixed;

        public static OplogConfig IoOptimised(string name)
        {
            return new OplogConfig()
            {
                Name = name,
                Quota = 4 * Units.MEGA,
                Fixed = true,
                BasePath = "ops"
            };
        }

        public static OplogConfig IoQuoted(string name, long q)
        {
            return new OplogConfig()
            {
                Name = name,
                Quota = q,
                Fixed = true,
                BasePath = "ops"
            };
        } 
    }

    public class Oplog : IDisposable
    {
        private readonly OplogConfig _config;

        public readonly List<ISegment> Segments;
        public HotSegment CurrentSegment;

        public Oplog(OplogConfig config)
        {
            _config = config;

            Segments = Directory
                .GetFiles(Path.Combine(_config.BasePath, "."), string.Format("{0}*.sf", _config.Name))
                .Select(x => ColdSegment.Load(x, _config.Name))
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

                var burner = new HotSegmentBurner(_config, last.Position);
                CurrentSegment = new HotSegment(_config.Quota, blocks) { Burner = burner, Position = last.Position };

                Segments.Add(CurrentSegment);
            }
        }

        public Oplog(string name = "") : this(OplogConfig.IoOptimised(name))
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

            return current + (Segments.Count - 1) * _config.Quota;
        }

        private void RollNewSegment()
        {
            if (CurrentSegment != null)
                CurrentSegment.Burner.Dispose();

            var position = Segments.Count * _config.Quota;

            CurrentSegment = new HotSegment(_config.Quota)
            {
                Burner = new HotSegmentBurner(_config, position),
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