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
            foreach (var file in Directory.GetFiles(".", "*.sf"))
                File.Delete(file);
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            // ...
        }
    }
}