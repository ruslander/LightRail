using System;
using System.Collections.Generic;
using System.Linq;
using LightRail;
using NUnit.Framework;

namespace Specs.Proc
{
    [TestFixture]
    public class FailRecoverySpec2 : SpecificationWithFile
    {
        [Test]
        public void Oplog()
        {
            var lw = new Oplog(Cfg4K);
            lw.Append(new Op() { Body = 333 });
            lw.Dispose();

            var lr = new Oplog(Cfg4K);
            var op = lw.Head();
            lr.Dispose();

            Assert.That(op.Body, Is.EqualTo(333));
        }
    }

    public class FailRecoveryProcessLog
    {
        public int Version = 0;
        public List<object> Changes = new List<object>();

        public void Apply(object evt)
        {
            Version++;
            Changes.Add(evt);

            ((dynamic)this).When((dynamic)evt);
        }

        public void Dispatch(object cmd)
        {
            ((dynamic)this).Handle((dynamic)cmd);
        }

        public List<object> Delta(int fromV)
        {
            return Changes.Skip(fromV).ToList();
        }

        public void LoadsFromHistory(IEnumerable<Event> history)
        {
            foreach (var e in history) 
                ((dynamic)this).When((dynamic)e);
        }
    }

    public class DistributedFeaturesRegistry : FailRecoveryProcessLog
    {
        private readonly Dictionary<string, FeatureState> _features = new Dictionary<string, FeatureState>();

        public void Handle(AddFeatureCommand cmd)
        {
            if (_features.Any(@switch => cmd.Name == @switch.Value.Name))
                return;

            Apply(new FeatureAdded()
            {
                Id = cmd.Id,
                Name = cmd.Name,
                Enabled = cmd.Enabled
            });
        }

        public void Handle(FlipFeatureCommand cmd)
        {
            if (_features.Any(@switch => cmd.Id == @switch.Key) == false)
                return;

            Apply(new FeatureFlipped() { Id = cmd.Id });
        }

        public void When(FeatureAdded addedEvt)
        {
            _features.Add(addedEvt.Id, new FeatureState()
            {
                Enabled = addedEvt.Enabled,
                Name = addedEvt.Name
            });
        }

        public void When(FeatureFlipped flippedEvt)
        {
            _features[flippedEvt.Id].Enabled = !_features[flippedEvt.Id].Enabled;
        }

        public bool Get(string key)
        {
            return _features.Single(@switch => key == @switch.Value.Name).Value.Enabled;
        }

        public IEnumerable<Feature> All()
        {
            return _features.Select(@switch => new Feature()
            {
                Id = @switch.Key,
                Enabled = @switch.Value.Enabled,
                Name = @switch.Value.Name
            });
        }
    }

    public class Feature
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    public class FeatureState
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    [Serializable]
    public class AddFeatureCommand
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Id { get; set; }
    }

    [Serializable]
    public class FlipFeatureCommand
    {
        public string Id { get; set; }
    }

    [Serializable]
    public class FeatureFlipped
    {
        public string Id { get; set; }
    }

    [Serializable]
    public class FeatureAdded
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Id { get; set; }
    }
}