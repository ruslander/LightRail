using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail.Core
{
    public class ColdSegment : ISegment
    {
        public long Position { get; set; }
        public string Name { get; set; }

        public IEnumerable<Block> FetchForward()
        {
            Reader.BaseStream.Position = 0;

            Console.WriteLine("Fetch file#{0} {1}", Position, Name);

            var blockBuff = new byte[Units.KILO * 4];

            while (Reader.Read(blockBuff, 0, blockBuff.Length) > 0)
            {
                yield return new Block(blockBuff);
            }
        }

        public IEnumerable<Block> FetchBackward()
        {
            Console.WriteLine("Fetch file#{0} {1}", Position, Reader.BaseStream.Length);

            var blockBuff = new byte[Units.KILO * 4];

            var offset = Reader.BaseStream.Length - blockBuff.Length;

            while (offset >= 0)
            {
                Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                Reader.Read(blockBuff, 0, blockBuff.Length);

                yield return new Block(blockBuff);

                offset = offset - blockBuff.Length;
            }
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