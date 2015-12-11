using System;
using System.Collections.Generic;
using LightRail;

namespace Specs.Proc
{
    public class FailRecoveryFsm
    {
        public int Version = 0;
        public List<object> Ops = new List<object>();

        public void Apply(object evt)
        {
            Version++;
            Ops.Add(evt);

            ((dynamic)this).When((dynamic)evt);
        }

        public IEnumerable<object> GetUncommittedOps()
        {
            return Ops;
        }

        public void MarkOpsAsCommitted()
        {
            Ops.Clear();
        }

        public void LoadsFromHistory(object history)
        {
            ((dynamic)this).When((dynamic)history);
        }
    }

    public class FailRecoveryFsmRepository<T> where T : FailRecoveryFsm, new()
    {
        private readonly Oplog _storage;

        public FailRecoveryFsmRepository(Oplog storage)
        {
            _storage = storage;
        }

        public void Save(FailRecoveryFsm fsm)
        {
            foreach (var change in fsm.GetUncommittedOps())
                _storage.Append(new Op() { Body = change });
        }

        public T GetById(Guid id)
        {
            var obj = new T();

            foreach (var op in _storage.Forward())
                obj.LoadsFromHistory(op.Body); ;

            return obj;
        }
    }
}