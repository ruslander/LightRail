using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StableStorage
{
    public class OpDataPersister : IDisposable
    {
        private readonly OplogConfig _config;

        public readonly List<ISegment> Segments;
        public HotSegment CurrentSegment;

        public OpDataPersister(OplogConfig config)
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

        public OpDataPersister(string name = "") : this(OplogConfig.IoOptimised(name))
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

        public IEnumerable<OpData> Forward(long position = 0, int sliceSize = int.MaxValue)
        {
            var delivered = 0;

            foreach (var segment in Segments)
            {
                foreach (Block block in segment.FetchForward())
                {
                    foreach (byte[] record in block.Forward())
                    {
                        var op = OpData.ReadFrom(record);

                        if (op.Position >= position && delivered < sliceSize)
                        {
                            delivered++;
                            yield return op;
                        }
                    }
                }
            }
        }

        public IEnumerable<OpData> Backward(long position = int.MaxValue, int sliceSize = int.MaxValue)
        {
            var delivered = 0;

            foreach (var segment in Segments.ToList().OrderByDescending(x => x.Position))
            {
                foreach (var block in segment.FetchBackward())
                {
                    foreach (var record in block.Forward().Reverse())
                    {
                        var op = OpData.ReadFrom(record);

                        if (op.Position <= position && delivered < sliceSize)
                        {
                            delivered++;
                            yield return op;
                        }
                    }
                }
            }
        }

        public OpData Head()
        {
            var sg = Segments.ToList().OrderByDescending(x => x.Position).First();
            var block = sg.FetchBackward().First();
            var record = block.Forward().Reverse().FirstOrDefault();

            if (record == null)
                return null;

            return OpData.ReadFrom(record);
        }

        public OpData Tail()
        {
            var sg = Segments.First();
            var block = sg.FetchForward().First();
            var record = block.Forward().FirstOrDefault();

            if (record == null)
                return null;

            return OpData.ReadFrom(record);
        }

        public void Dispose()
        {
            if (CurrentSegment != null)
                CurrentSegment.Burner.Dispose();
        }
    }
}