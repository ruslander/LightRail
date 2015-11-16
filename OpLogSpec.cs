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
        public void OpWrite()
        {
            var msg = "this is the hello world";

            var output = Pack(msg);

            Console.WriteLine("Packed :" + output.Length);


            var result = Unpack(output);

            Console.WriteLine(result);
        }

        private object Unpack(byte[] output)
        {
            using (var memStream = new MemoryStream(output))
            using (var reader = new BinaryReader(memStream))
            {
                var length = reader.ReadInt64();
                var delivered = reader.ReadByte();
                var hash = reader.ReadBytes(16);

                Console.WriteLine("Position : " + memStream.Position);


                var payloadLength = length - hash.Length - 1;

                var payload = reader.ReadBytes((int)payloadLength);

                Console.WriteLine("Hash :" + hash.Length);
                Console.WriteLine("Payload : " + payload.Length);

                return FromStream(new MemoryStream(payload));
            }
        }

        private byte[] Pack(string msg)
        {
            using (var memStream = new MemoryStream())
            using (var writer = new BinaryWriter(memStream))
            {
                var payload = ToStream(msg);
                var hash = GetHashFor(payload);
                var length = 1 + payload.Length + hash.Length;

                Console.WriteLine("Hash :" + hash.Length);
                Console.WriteLine("Payload : " + payload.Length);
                Console.WriteLine("Calc length :" + length);

                writer.Write(length);

                writer.Write((byte) 0);
                writer.Write(hash);

                Console.WriteLine("Position : " + memStream.Position);

                writer.Write(payload.GetBuffer(), 0, (int) payload.Length);

                return memStream.GetBuffer();
            }
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

        public static byte[] GetHashFor(Stream s)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(s);
        }

        private static string NamingScheme(string prefix, int position)
        {
            return string.Format("{0}{1}.sf", prefix, position.ToString("D12"));
        }
    }

    public class Op
    {
         
    }
}