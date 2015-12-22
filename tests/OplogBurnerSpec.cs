using System.IO;
using System.Linq;
using StableStorage;
using StableStorage.Specs.Io;
using NUnit.Framework;

namespace Specs
{
    [TestFixture]
    public class OplogBurnerSpec : SpecificationWithFile
    {
        [Test]
        public void BurnTest()
        {
            var c1 = new byte[20];
            ByteArray.FillArrayRandomly(c1);

            var burner = new HotSegmentBurner(QuotedAs(100), 1);
            burner.Burn(new Block(c1), 1);
            burner.Dispose();

            var f1 = File.ReadAllBytes(burner.Path);

            Assert.That(f1.Take(20).ToArray(), Is.EqualTo(c1));
        }

        [Test]
        public void Burn2BlocksTest()
        {
            var c1 = new byte[20];
            ByteArray.FillArrayRandomly(c1);
            
            var c2 = new byte[20];
            ByteArray.FillArrayRandomly(c2);

            var burner = new HotSegmentBurner(QuotedAs(100), 2);
            burner.Burn(new Block(c1), 1);
            burner.Burn(new Block(c2), 2);
            burner.Dispose();

            var content = File.ReadAllBytes(burner.Path);

            Assert.That(content.Take(20).ToArray(), Is.EqualTo(c1));
            Assert.That(content.Skip(20).Take(20).ToArray(), Is.EqualTo(c2));
        }

        [Test]
        public void BurnOverwriteTest()
        {
            var c1 = new byte[20];
            ByteArray.FillArrayRandomly(c1);

            var c2 = new byte[20];
            ByteArray.FillArrayRandomly(c2);

            var burner = new HotSegmentBurner(QuotedAs(100), 3);
            burner.Burn(new Block(c1), 1);
            burner.Burn(new Block(c2), 1);
            burner.Dispose();

            var f1 = File.ReadAllBytes(burner.Path);

            Assert.That(f1.Take(20), Is.EqualTo(c2));
            Assert.That(f1.Take(20).Count(), Is.EqualTo(20));
        }
    }
}