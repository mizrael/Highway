using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Persistence
{
    public interface ISagaStateRepository
    {
        Task<TD> FindByCorrelationIdAsync<TD>(Guid correlationId, ITransaction transaction = null, CancellationToken cancellationToken = default) where TD : SagaState;
        Task SaveAsync<TD>(Guid correlationId, TD state, ITransaction transaction = null, CancellationToken cancellationToken = default) where TD : SagaState;
    }
}