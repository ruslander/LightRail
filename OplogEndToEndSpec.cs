using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace LightRail
{
    [TestFixture]
    public class OplogEndToEndSpec
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            foreach (var file in Directory.GetFiles(".", "*.sf"))
                File.Delete(file);
        }

        [Test]
        public void append_1_mln()
        {
            var sut = new Oplog("a", new SegmentManager("a"));

            var watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000000; i++)
            {
                sut.Append(Encoding.Unicode.GetBytes("hello world"));
            }

            watch.Stop();

            Console.WriteLine();
            Console.WriteLine("Ops                  : " + 1000000);
            Console.WriteLine("Duraton              : " + TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds));
            Console.WriteLine("Sync     (millisec)  : " + (((float)watch.ElapsedMilliseconds / 1000000)));
        }
    }
}