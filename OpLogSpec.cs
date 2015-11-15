using NUnit.Framework;

namespace LightRail
{
    [TestFixture]
    public class OpLogSpec
    {
        [Test]
        public void NamingWithPaddingZeros()
        {
            Assert.That(NamingScheme("out.", 0),       Is.EqualTo("out.000000000000.sf"));
            Assert.That(NamingScheme("in.", 4000000),  Is.EqualTo("in.000004000000.sf"));
        }

        private static string NamingScheme(string prefix, int position)
        {
            return string.Format("{0}{1}.sf", prefix, position.ToString("D12"));
        }
    }
}