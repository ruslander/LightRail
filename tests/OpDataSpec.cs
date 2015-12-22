using System;
using System.IO;
using System.Security.Cryptography;
using StableStorage;
using NUnit.Framework;

namespace Specs
{
    /*
     
     message length : 8 bytes (value: 16+n) 
        md5            : 16 bytes
        payload        : n bytes

     */

    [TestFixture]
    public class OpDataSpec
    {
        readonly byte[] _empty = new byte[] { };
        readonly byte[] _oneByte = new byte[] { 1 };

        [Test]
        public void Etalon()
        {
            Assert.That(_empty.Length, Is.EqualTo(0));
            Assert.That(_oneByte.Length, Is.EqualTo(1));
        }

        [Test]
        public void Read()
        {
            var wS = new MemoryStream();
            var writer = new BinaryWriter(wS);

            var initailMd5 = MD5.Create().ComputeHash(new MemoryStream(_oneByte));

            writer.Write((long)11111);
            writer.Write(initailMd5);
            writer.Write(_oneByte);

            //var rS = new MemoryStream(wS.ToArray());
            //var reader = new BinaryReader(rS);

            var op = OpData.ReadFrom(wS.ToArray());

            Assert.That(op.Hash, Is.EqualTo(initailMd5));
            Assert.That(op.Payload, Is.EqualTo(_oneByte));
            Assert.That(op.Length, Is.EqualTo(25));
            Assert.That(op.Position, Is.EqualTo(11111));
        }

        [Test]
        public void Write()
        {
            var o = new OpData(_oneByte,1);

            var storage = new MemoryStream();
            var writer = new BinaryWriter(storage);

            o.WriteTo(writer);

            Assert.That(storage.ToArray().Length, Is.EqualTo(8+16+1));
        }

        [Test]
        public void WriteFormat()
        {
            var o = new OpData(_oneByte, 1);

            Assert.That(BitConverter.GetBytes(o.Position).Length, Is.EqualTo(8));
            Assert.That(o.Hash.Length, Is.EqualTo(16));
            Assert.That(o.Payload.Length, Is.EqualTo(1));
        }

        [Test]
        public void LongLength_8bytes()
        {
            var storage = new MemoryStream();

            var writer = new BinaryWriter(storage);
            writer.Write((long)1);

            Assert.That(storage.ToArray().Length, Is.EqualTo(8));
        }

        [Test]
        public void Md5_16bytes()
        {
            using (var md5 = MD5.Create())
            {
                var emptyHashV1 = md5.ComputeHash(new MemoryStream(_empty));
                var emptyHashV2 = md5.ComputeHash(new MemoryStream(_empty));

                Assert.That(emptyHashV1.Length, Is.EqualTo(16));
                Assert.That(emptyHashV2.Length, Is.EqualTo(16));

                Assert.That(emptyHashV1, Is.EqualTo(emptyHashV2));


                var oneHashV1 = md5.ComputeHash(new MemoryStream(_oneByte));
                var oneHashV2 = md5.ComputeHash(new MemoryStream(_oneByte));

                Assert.That(oneHashV1.Length, Is.EqualTo(16));
                Assert.That(oneHashV2.Length, Is.EqualTo(16));

                Assert.That(oneHashV1, Is.EqualTo(oneHashV2));

                Assert.That(oneHashV1, Is.Not.EqualTo(emptyHashV1));
            }
        }
    }
}