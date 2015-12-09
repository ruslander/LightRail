using System;
using System.Collections.Generic;
using System.Linq;

namespace LightRail
{
    public class Oplog : IDisposable
    {
        public OpDataPersister Advanced;

        public Oplog(string name = "")
        {
            Advanced = new OpDataPersister(name);
        }
        
        public Oplog(OplogConfig config)
        {
            Advanced = new OpDataPersister(config);
        }

        public long Append(Op op)
        {
            return Advanced.Append(op.ToBinary());
        }
       
        public IEnumerable<Op> Forward(long position = 0, int sliceSize = int.MaxValue)
        {
            return Advanced.Forward(position, sliceSize).Select(x => Op.FromBinary(x.Payload));
        }

        public IEnumerable<Op> Backward(long position = int.MaxValue, int sliceSize = int.MaxValue)
        {
            return Advanced.Backward(position, sliceSize).Select(x => Op.FromBinary(x.Payload));
        }

        public Op Head()
        {
            var x = Advanced.Head();
            return x == null ? null : Op.FromBinary(x.Payload);
        }

        public Op Tail()
        {
            var x = Advanced.Tail();
            return x == null ? null : Op.FromBinary(x.Payload);
        }

        public void Dispose()
        {
            Advanced.Dispose();
        }
    }
}