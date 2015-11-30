using System;
using System.IO;
using NUnit.Framework;

namespace Specs
{
    public class SpecificationWithFile
    {
        protected string Filename;

        /*[TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            foreach (var file in Directory.GetFiles(".", "*.sf"))
                File.Delete(file);
        }*/

        [SetUp]
        public virtual void SetUp()
        {
            var typeName = GetType().Name.Length > 30 ? GetType().Name.Substring(0, 30) : GetType().Name;
            Filename = string.Format("{0}-{1}", typeName, Guid.NewGuid());
        }
        
        [TearDown]
        public virtual void TearDown()
        {
            if (File.Exists(Filename))
                File.Delete(Filename);
        }
    }
}