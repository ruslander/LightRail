using System;
using System.IO;
using LightRail;
using NUnit.Framework;

namespace Specs
{
    public class SpecificationWithFile
    {
        protected string Filename;

        protected OplogConfig Cfg4K;
        protected OplogConfig Cfg4Mb;

        [SetUp]
        public virtual void SetUp()
        {
            var typeName = GetType().Name.Length > 30 ? GetType().Name.Substring(0, 30) : GetType().Name;
            Filename = string.Format("{0}-{1}", typeName, Guid.NewGuid());

            Cfg4K = new OplogConfig()
            {
                Name = Filename,
                Quota = 4 * Units.KILO,
                Fixed = true,
                BasePath = "utt"
            };

            Cfg4Mb = new OplogConfig()
            {
                Name = Filename,
                Quota = 4 * Units.MEGA,
                Fixed = true,
                BasePath = "utt"
            };

        }
        
        [TearDown]
        public virtual void TearDown()
        {
            if (File.Exists(Filename))
                File.Delete(Filename);
        }

        protected bool FileExists(string ext)
        {
            return File.Exists("utt\\" + Filename + ext);
        }

        protected OplogConfig QuotedAs(long i)
        {
            var config = OplogConfig.IoQuoted(Filename, i);
            config.BasePath = "utt";

            return config;
        }
    }
}