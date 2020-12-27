using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Persistence;

namespace Highway.Persistence.InMemory
{
    public class InMemorySagaStateRepository : ISagaStateRepository
    {
        private readonly ConcurrentDictionary<Guid, SagaState> _items;

        public InMemorySagaStateRepository()
        {
            _items = new ConcurrentDictionary<Guid, SagaState>();
        }

        public Task<(TD state, Guid lockId)> LockAsync<TD>(Guid id, TD newEntity = default, CancellationToken cancellationToken = default) 
            where TD : SagaState
        {
            var state = _items.AddOrUpdate(id, k => newEntity, (k,v) => v) as TD;

            return Task.FromResult( (state, Guid.NewGuid()) );
        }

        public Task UpdateAsync<TD>(TD state, Guid lockId, bool releaseLock = false, CancellationToken cancellationToken = default) where TD : SagaState
        {
            _items.AddOrUpdate(state.Id, state, (k, v) => state);
            return Task.CompletedTask;
        }
    }
}