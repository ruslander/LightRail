using System;
using System.Collections.Generic;
using System.IO;

namespace LightRail.Core
{
    public class SegmentFullException : Exception { }
    public class Segment
    {
        private readonly long _capacity;

        public readonly List<Block> Blocks = new List<Block>();
        Block _current = Block.New();

        public Segment(long capacity)
        {
            _capacity = capacity;
        }

        public void Append(byte[] next)
        {
            var op = new Op(next);

            var storage = new MemoryStream();
            var writer = new BinaryWriter(storage);

            op.WriteTo(writer);

            try
            {
                _current.Append(storage.ToArray());
            }
            catch (BlockFullException)
            {
                if ((Blocks.Count + 1) * _current.Payload.Length > _capacity)
                    throw new SegmentFullException();

                Blocks.Add(_current);

                _current = Block.New();
                _current.Append(storage.ToArray());
            }
        }
    }
}