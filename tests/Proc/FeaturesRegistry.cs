using System;
using System.Collections.Generic;
using System.Linq;

namespace Specs.Proc
{
    public class FeaturesRegistry : FailRecoveryFsm
    {
        private readonly Dictionary<string, FeatureState> _features = new Dictionary<string, FeatureState>();

        public void Handle(AddFeatureCommand cmd)
        {
            if (_features.Any(@switch => cmd.Id == @switch.Key))
                return;

            Apply(new FeatureAdded()
            {
                Id = cmd.Id,
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
            });
        }

        public void When(FeatureFlipped flippedEvt)
        {
            _features[flippedEvt.Id].Enabled = !_features[flippedEvt.Id].Enabled;
        }

        public bool Get(string key)
        {
            return _features[key].Enabled;
        }

        public IEnumerable<Feature> All()
        {
            return _features.Select(@switch => new Feature()
            {
                Id = @switch.Key,
                Enabled = @switch.Value.Enabled,
            });
        }
    }

    public class Feature
    {
        public string Id { get; set; }
        public bool Enabled { get; set; }
    }

    public class FeatureState
    {
        public bool Enabled { get; set; }
    }

    [Serializable]
    public class AddFeatureCommand
    {
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
        public bool Enabled { get; set; }
        public string Id { get; set; }
    }
}