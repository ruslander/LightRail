using System;
using System.IO;

namespace LightRail.Core
{
    public class HotSegmentBurner : IDisposable
    {
        readonly FileStream _fsSegment;
        public string Path;

        public HotSegmentBurner(string name, long capacity, long position)
        {
            Path = string.Format("{0}{1}.sf", name, position.ToString("D12"));;

            _fsSegment = new FileStream(
                Path, 
                FileMode.OpenOrCreate, 
                FileAccess.ReadWrite, 
                FileShare.ReadWrite,
                4096, 
                FileOptions.WriteThrough);

            _fsSegment.SetLength(capacity);
        }

        public void Burn(Block current, int count)
        {
            var blockLength = current.Payload.Length;
            var offset = blockLength * (count - 1);
            
            if (_fsSegment.Position != offset)
            {
                _fsSegment.Seek(offset, SeekOrigin.Begin);
            }

            _fsSegment.Write(current.Payload, 0, blockLength);
            _fsSegment.Flush(true);

            //Console.WriteLine("Burn [{0}] {1}", count, Path);
        }

        public void Dispose()
        {
            _fsSegment.Flush(true);
            _fsSegment.Close();
        }
    }
}