using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail.Core
{
    public class HotSegmentFullException : Exception { }

    public interface ISegment
    {
        long Position { get; set; }
        IEnumerable<Block> Fetch();
        void Dispose();
    }

    public class ColdSegment : ISegment
    {
        public long Position { get; set; }
        public string Name { get; set; }

        public IEnumerable<Block> Fetch()
        {
            Reader.BaseStream.Position = 0;

            Console.WriteLine("Fetch {0} {1}", Position, Name);

            var blockBuff = new byte[Units.KILO * 4];

            while (Reader.Read(blockBuff, 0, blockBuff.Length) > 0)
            {
                yield return new Block(blockBuff);
            }
        }

        public void Dispose()
        {
            Reader.Close();
        }

        public BinaryReader Reader { get; set; }

        public static ISegment AsReadonly(string path, string prefix = "")
        {
            var name = Path.GetFileName(path);
            var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            var position = long.Parse(name.Replace(prefix,"").Split('.')[0]); // 000014680064.sf

            return new ColdSegment()
            {
                Reader = reader,
                Position = position,
                Name = name
            };
        }

        public override string ToString()
        {
            return string.Format("Blocks {0} Records {1}", Fetch().Count(), Fetch().Select(x => x.Records().Count).Sum());
        }
    }

    public class HotSegment : ISegment
    {
        public HotSegmentBurner Burner { get; set; }
        public long Position { get; set; }
        public List<Block> Blocks { get; set; }

        public IEnumerable<Block> Fetch()
        {
            return Blocks.AsEnumerable();
        }

        public void Dispose()
        {
            Burner.Dispose();
        }

        private readonly long _capacity;

        Block _current;

        public HotSegment(long capacity, List<Block> blocks)
        {
            _capacity = capacity;

            Blocks = blocks;
            _current = blocks.Last();
        }

        public HotSegment(long capacity)
        {
            _capacity = capacity;
            
            Blocks = new List<Block>();

            RollCurrentBlock();
        }

        public void Append(byte[] next)
        {
            var op = new Op(next);

            var storage = new MemoryStream();
            var writer = new BinaryWriter(storage);

            op.WriteTo(writer);

            try
            {
                BurnCurrentBlock(storage.ToArray());
            }
            catch (BlockFullException)
            {
                if ((Blocks.Count + 1) * _current.Payload.Length > _capacity)
                    throw new HotSegmentFullException();

                RollCurrentBlock();

                BurnCurrentBlock(storage.ToArray());
            }
        }

        private void BurnCurrentBlock(byte[] record)
        {
            _current.Append(record);
            Burner.Burn(_current, Blocks.Count);
        }

        private void RollCurrentBlock()
        {
            _current = Block.New();
            Blocks.Add(_current);
        }

        public override string ToString()
        {
            return string.Format("Blocks {0} Records {1}", Blocks.Count, Blocks.Select(x=>x.Records().Count).Sum());
        }
    }
}