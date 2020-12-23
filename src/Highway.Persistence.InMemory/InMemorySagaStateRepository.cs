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

        public Task<TD> FindByCorrelationIdAsync<TD>(Guid correlationId, ITransaction transaction = null, CancellationToken cancellationToken = default)
            where TD : SagaState
        {
            var state = _items.GetValueOrDefault(correlationId);
            return Task.FromResult(state as TD);
        }

        public Task SaveAsync<TD>(Guid correlationId, TD state, ITransaction transaction = null, CancellationToken cancellationToken = default)
            where TD : SagaState
        {
            _items.AddOrUpdate(correlationId, state, (k, v) => state);
            return Task.CompletedTask;
        }
    }
}