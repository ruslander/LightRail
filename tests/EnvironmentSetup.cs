using System.IO;
using NUnit.Framework;

namespace Specs
{
    [SetUpFixture]
    public class EnvironmentSetup
    {
        [SetUp]
        public void RunBeforeAnyTests()
        {
            if (!Directory.Exists("utt"))
                Directory.CreateDirectory("utt");

            foreach (var file in Directory.GetFiles("utt", "*.sf"))
                File.Delete(file);
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            // ...
        }
    }
}