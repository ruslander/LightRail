using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LightRail;
using NUnit.Framework;

namespace Specs
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

        [Test,Ignore]
        public void append_1_mln()
        {
            const int ops = 1000000;
            //const int ops = 100;

            var wl = new Oplog("a");
            var wlWatch = Stopwatch.StartNew();

            for (int i = 0; i < ops; i++)
                wl.Append(Encoding.Unicode.GetBytes(i.ToString()));

            wlWatch.Stop();
            wl.Dispose();

            var rl = new Oplog("a");

            var rlWatch = Stopwatch.StartNew();
            var reads = rl.Forward().Count();
            rlWatch.Stop();


            Console.WriteLine("Writes            : " + ops);
            Console.WriteLine("Write Timespan    : " + TimeSpan.FromMilliseconds(wlWatch.ElapsedMilliseconds));
            Console.WriteLine("Write Duraton     : " + (((float)wlWatch.ElapsedMilliseconds / 1000000)));

            Console.WriteLine("");

            Console.WriteLine("Reads             : " + reads);
            Console.WriteLine("Reads Timespan    : " + TimeSpan.FromMilliseconds(rlWatch.ElapsedMilliseconds));
            Console.WriteLine("Read Duraton      : " + (((float)rlWatch.ElapsedMilliseconds / 1000000)));
        }
    }
}