using System.IO;
using System.Security.Cryptography;

namespace LightRail.Core
{
    public class Op
    {
        public byte[] Payload { get; private set; }
        public byte[] Hash { get; private set; }
        public long Length { get; private set; }
        public long Position { get; private set; }

        public Op(byte[] payload)
        {
            Payload = payload;
            Hash = GetHashFor(payload);
            Length = Payload.Length + Hash.Length + 8;
        }

        public Op(byte[] payload, byte[] hash, long position)
        {
            Payload = payload;
            Hash = hash;
            Length = Payload.Length + Hash.Length + 8;
            Position = position;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Length);
            writer.Write(Hash);
            writer.Write(Payload);
        }

        public static Op ReadFrom(BinaryReader reader)
        {
            var localPosition = reader.BaseStream.Position;

            var length = reader.ReadInt64();
            var hash = reader.ReadBytes(16);

            var payloadLength = length - hash.Length - 8;
            var payload = reader.ReadBytes((int)payloadLength);

            return new Op(payload, hash, localPosition);
        }

        public static Op ReadFrom(byte[] record, long position)
        {
            var reader = new BinaryReader(new MemoryStream(record));

            var length = reader.ReadInt64();
            var hash = reader.ReadBytes(16);

            var payloadLength = length - hash.Length - 8;
            var payload = reader.ReadBytes((int)payloadLength);

            return new Op(payload, hash, position);
        }

        public static byte[] GetHashFor(byte[] payload)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(new MemoryStream(payload));
        }
    }
}