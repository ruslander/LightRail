using System;
using System.IO;
using System.Text;
using LightRail.Core;
using NUnit.Framework;

namespace LightRail
{
    [TestFixture]
    public class OpLogV0Spec
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            foreach (var file in Directory.GetFiles(".", "*.sf"))
                File.Delete(file);
        }

        [Test]
        public void Append()
        {
            var append = new OplogV0("");

            for (var w = 0; w < 1000*100; w++)
            {
                var pos = append.Append(new[] { (byte)w });
                
                if(pos % 1000 == 0)
                    Console.WriteLine(pos);
            }

            append.Dispose();

            var read = new OplogV0("");
            var op = read.Read(1794000);

            Assert.That(op, Is.Not.Null);

        }

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
                var msgStream = Encoding.Unicode.GetBytes(quote);
                var op = new Op(msgStream);

                op.WriteTo(writer);

                Console.WriteLine("w" + storage.Position);
            }

            storage.Position = 0;

            var reader = new BinaryReader(storage);
            foreach (var quote in quotes)
            {
                var result = Op.ReadFrom(reader);
                var org2 = Encoding.Unicode.GetString(result.Payload);

                Console.WriteLine("r" + storage.Position);
                Assert.That(quote, Is.EqualTo(org2));
            }
        }

        [Test]
        public void SingleOpWriteRead()
        {
            var storage = new MemoryStream();

            const string msg = "this is the hello world";
            
            var msgStream = Encoding.Unicode.GetBytes(msg);
            var op = new Op(msgStream);

            var writer = new BinaryWriter(storage);
            op.WriteTo(writer);

            storage.Position = 0;
            var reader = new BinaryReader(storage);
            var result = Op.ReadFrom(reader);

            var org2 = Encoding.Unicode.GetString(result.Payload);

            Assert.That(msg, Is.EqualTo(org2));
        }

        [Test]
        public void NamingWithPaddingZeros()
        {
            Assert.That(new OplogV0("out.").NamingScheme(0), Is.EqualTo("out.000000000000.sf"));
            Assert.That(new OplogV0("in.").NamingScheme(4000000),  Is.EqualTo("in.000004000000.sf"));
        }
    }
}