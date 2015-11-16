using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using NUnit.Framework;

namespace LightRail
{
    [TestFixture]
    public class OpLogSpec
    {
        [Test]
        public void MultiOpWriteRead()
        {
            var storage = new MemoryStream();

            var quotes = new string[]
            {
                "Frugality without creativity is deprivation.",
                "Use, do not abuse; neither abstinence nor excess ever renders man happy.",
                "Prosperity is only an instrument to be used, not a deity to be worshipped.",
                "It's very important that we start creating new content again. We can only build on nostalgia so much before we have nothing left to build on.",
            };

            var writer = new BinaryWriter(storage);
            foreach (var quote in quotes)
            {
                var msgStream = ToStream(quote);
                var op = new Op(msgStream.GetBuffer());

                op.WriteTo(writer);

                Console.WriteLine("w" + storage.Position);
            }

            storage.Position = 0;

            var reader = new BinaryReader(storage);
            foreach (var quote in quotes)
            {
                var result = Op.ReadFrom(reader);
                var org2 = FromStream(new MemoryStream(result.Payload));

                Console.WriteLine("r" + storage.Position);
                Assert.That(quote, Is.EqualTo(org2));
            }
        }

        [Test]
        public void SingleOpWriteRead()
        {
            var storage = new MemoryStream();

            const string msg = "this is the hello world";
            var msgStream = ToStream(msg);
            var op = new Op(msgStream.GetBuffer());

            var writer = new BinaryWriter(storage);
            op.WriteTo(writer);

            storage.Position = 0;
            var reader = new BinaryReader(storage);
            var result = Op.ReadFrom(reader);

            var org2 = FromStream(new MemoryStream(result.Payload));

            Assert.That(msg, Is.EqualTo(org2));
        }

        [Test]
        public void NamingWithPaddingZeros()
        {
            Assert.That(NamingScheme("out.", 0),       Is.EqualTo("out.000000000000.sf"));
            Assert.That(NamingScheme("in.", 4000000),  Is.EqualTo("in.000004000000.sf"));
        }

        public MemoryStream ToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        public object FromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream);
        }

        private static string NamingScheme(string prefix, int position)
        {
            return string.Format("{0}{1}.sf", prefix, position.ToString("D12"));
        }
    }

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
            Length = Payload.Length + Hash.Length + 1;
        }

        public Op(byte[] payload, byte[] hash, long length)
        {
            Payload = payload;
            Hash = hash;
            Length = length;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Length);
            writer.Write((byte)0);
            writer.Write(Hash);
            writer.Write(Payload);
        }

        public static Op ReadFrom(BinaryReader reader)
        {
            var length = reader.ReadInt64();
            var delivered = reader.ReadByte();
            var hash = reader.ReadBytes(16);

            var payloadLength = length - hash.Length - 1;
            var payload = reader.ReadBytes((int)payloadLength);

            return new Op(payload, hash, length);
        }

        public static byte[] GetHashFor(byte[] payload)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(new MemoryStream(payload));
        }
    }
}