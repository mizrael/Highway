using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Persistence
{
    public interface ISagaStateRepository<TD>
        where TD : SagaState
    {
        Task<TD> FindByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
        Task SaveAsync(Guid correlationId, TD state, CancellationToken cancellationToken = default);
    }
}