using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StableStorage
{
    public class ColdSegment : ISegment
    {
        public long Position { get; set; }
        public string Name { get; set; }
        public BinaryReader Reader { get; set; }

        public IEnumerable<Block> FetchForward()
        {
            Reader.BaseStream.Position = 0;

            Console.WriteLine("Fetch file#{0} {1}", Position, Name);

            var blockBuffer = new byte[Units.KILO * 4];

            while (Reader.Read(blockBuffer, 0, blockBuffer.Length) > 0)
            {
                var block = new Block(blockBuffer.ToArray());
                
                if (block.Records().Count == 0)
                    break;

                yield return block;
            }
        }

        public IEnumerable<Block> FetchBackward()
        {
            Console.WriteLine("Fetch file#{0} {1}", Position, Reader.BaseStream.Length);

            var blockBuffer = new byte[Units.KILO * 4];

            var idx = Reader.BaseStream.Length - blockBuffer.Length;

            while (idx >= 0)
            {
                Reader.BaseStream.Seek(idx, SeekOrigin.Begin);
                Reader.Read(blockBuffer, 0, blockBuffer.Length);

                yield return new Block(blockBuffer.ToArray());

                idx = idx - blockBuffer.Length;
            }
        }

        public static ISegment Load(string path, string prefix = "")
        {
            var name = Path.GetFileName(path);
            var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,4096));
            var position = long.Parse(name.Replace(prefix,"").Split('.')[0]); // 000014680064.sf

            reader.BaseStream.Position = 0;

            return new ColdSegment()
            {
                Reader = reader,
                Position = position,
                Name = name
            };
        }

        public void Dispose()
        {
            Reader.Close();
        }

        public override string ToString()
        {
            return string.Format("Blocks {0} Records {1}", FetchForward().Count(), FetchForward().Select(x => x.Records().Count).Sum());
        }
    }
}