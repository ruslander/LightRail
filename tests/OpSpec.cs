using System.IO;
using System.Security.Cryptography;
using LightRail;
using NUnit.Framework;

namespace Specs
{
    /*
     
     message length : 8 bytes (value: 16+n) 
        md5            : 16 bytes
        payload        : n bytes

     */

    [TestFixture]
    public class OpSpec
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

            writer.Write((long)8 + 16 + 1);
            writer.Write(initailMd5);
            writer.Write(_oneByte);

            var rS = new MemoryStream(wS.ToArray());
            var reader = new BinaryReader(rS);

            var op = Op.ReadFrom(reader);

            Assert.That(op.Hash, Is.EqualTo(initailMd5));
            Assert.That(op.Payload, Is.EqualTo(_oneByte));
            Assert.That(op.Length, Is.EqualTo(8 + 16 + 1));
            Assert.That(op.Position, Is.EqualTo(0));
        }

        [Test]
        public void ReadWillRespectPosition()
        {
            var wS = new MemoryStream();
            var writer = new BinaryWriter(wS);

            writer.Write((long)8 + 16 + 1);
            writer.Write(MD5.Create().ComputeHash(new MemoryStream(_oneByte)));
            writer.Write(_oneByte);

            writer.Write((long)8 + 16 + 1);
            writer.Write(MD5.Create().ComputeHash(new MemoryStream(_oneByte)));
            writer.Write(_oneByte);

            wS.Position = 0;

            var reader = new BinaryReader(wS);

            var o1 = Op.ReadFrom(reader);
            var o2 = Op.ReadFrom(reader);

            Assert.That(o1.Position, Is.EqualTo(0));
            Assert.That(o1.Length, Is.EqualTo(25));

            Assert.That(o2.Position, Is.EqualTo(25));
            Assert.That(o2.Length, Is.EqualTo(25));
        }

        [Test]
        public void Write()
        {
            var o = new Op(_oneByte);

            var storage = new MemoryStream();
            var writer = new BinaryWriter(storage);

            o.WriteTo(writer);

            Assert.That(storage.ToArray().Length, Is.EqualTo(8+16+1));
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