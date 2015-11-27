using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LightRail.Core;
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
            var writterLog = new Oplog();

            var writterWatch = Stopwatch.StartNew();

            const int ops = 1000000;

            for (int i = 0; i < ops; i++)
                writterLog.Append(Encoding.Unicode.GetBytes(i.ToString()));

            writterWatch.Stop();

            writterLog.Flush();

            var readerLog = new Oplog();

            var readerWatch = Stopwatch.StartNew();
            var reads = readerLog.Forward().Count();
            readerWatch.Stop();


            Console.WriteLine("Writes            : " + ops);
            Console.WriteLine("Write Timespan    : " + TimeSpan.FromMilliseconds(writterWatch.ElapsedMilliseconds));
            Console.WriteLine("Write Duraton     : " + (((float)writterWatch.ElapsedMilliseconds / 1000000)));
            
            Console.WriteLine("");

            Console.WriteLine("Reads             : " + reads);
            Console.WriteLine("Reads Timespan    : " + TimeSpan.FromMilliseconds(readerWatch.ElapsedMilliseconds));
            Console.WriteLine("Read Duraton      : " + (((float)readerWatch.ElapsedMilliseconds / 1000000)));
        }
    }
}