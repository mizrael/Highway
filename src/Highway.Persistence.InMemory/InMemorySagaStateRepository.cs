using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Persistence;

namespace Highway.Persistence.InMemory
{
    public class InMemorySagaStateRepository<TD> : ISagaStateRepository<TD>
        where TD : SagaState
    {
        private readonly ConcurrentDictionary<Guid, TD> _items;

        public InMemorySagaStateRepository()
        {
            _items = new ConcurrentDictionary<Guid, TD>();
        }

        public Task<TD> FindByCorrelationIdAsync(Guid correlationId)
        {
            var state = _items.GetValueOrDefault(correlationId);
            return Task.FromResult(state);
        }

        public Task SaveAsync(Guid correlationId, TD state)
        {
            _items.AddOrUpdate(correlationId, state, (k, v) => state);
            return Task.CompletedTask;
        }
    }
}