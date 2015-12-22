using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace StableStorage
{
    [Serializable]
    public class Op
    {
        public Op()
        {
            Headers = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Headers { get; set; }
        public object Body { get; set; }

        private static readonly IFormatter Formatter = new BinaryFormatter();

        public byte[] ToBinary()
        {
            using (var ms = new MemoryStream())
            {
                Formatter.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public static Op FromBinary(byte[] input)
        {
            using (var memStream = new MemoryStream())
            {
                memStream.Write(input, 0, input.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (Op)Formatter.Deserialize(memStream);
            }
        }
    }
}