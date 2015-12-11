using System;
using LightRail;
using NUnit.Framework;

namespace Specs.Proc
{
    [TestFixture]
    public class FeaturesRegistryFsmSpec : SpecificationWithFile
    {
        [Test]
        public void OplogAsStorage()
        {
            var log = new Oplog(Cfg4K);
            var repository = new FailRecoveryFsmRepository<FeaturesRegistry>(log);

            var sut = repository.GetById(new Guid());
            sut.Handle(new AddFeatureCommand(){Id = "beta_users_suggestions"});
            sut.Handle(new AddFeatureCommand(){Id = "beta_users_wellcome_email_greetings"});
            sut.Handle(new FlipFeatureCommand() { Id = "beta_users_wellcome_email_greetings" });

            repository.Save(sut);
            sut.MarkOpsAsCommitted();

            var resurrected = repository.GetById(new Guid());

            Assert.That(sut.Get("beta_users_suggestions"), Is.False);
            Assert.That(sut.Get("beta_users_wellcome_email_greetings"), Is.True);

            Assert.That(resurrected.Get("beta_users_suggestions"), Is.False);
            Assert.That(resurrected.Get("beta_users_wellcome_email_greetings"), Is.True);
        }
    }

    
}