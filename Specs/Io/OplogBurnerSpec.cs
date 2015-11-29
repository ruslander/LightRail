using System.IO;
using System.Linq;
using LightRail.Core;
using NUnit.Framework;

namespace LightRail.Specs.Io
{
    [TestFixture]
    public class OplogBurnerSpec
    {
        private int cap = 100;

        [TestFixtureSetUp]
        public void SetUp()
        {
            foreach (var file in Directory.GetFiles(".", "*.sf"))
                File.Delete(file);
        }

        [Test]
        public void BurnTest()
        {
            var c1 = new byte[20];
            ByteArray.FillArrayRandomly(c1);

            var burner = new HotSegmentBurner("a", cap, 1);
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

            var burner = new HotSegmentBurner("b",cap, 2);
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

            var burner = new HotSegmentBurner("c",cap, 3);
            burner.Burn(new Block(c1), 1);
            burner.Burn(new Block(c2), 1);
            burner.Dispose();

            var f1 = File.ReadAllBytes(burner.Path);

            Assert.That(f1.Take(20), Is.EqualTo(c2));
            Assert.That(f1.Take(20).Count(), Is.EqualTo(20));
        }
    }
}