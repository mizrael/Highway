using System;
using System.Threading.Tasks;

namespace Highway.Core.Persistence
{
    public interface ISagaStateRepository<TD>
        where TD : SagaState
    {
        Task<TD> FindByCorrelationIdAsync(Guid correlationId);
        Task SaveAsync(Guid correlationId, TD state);
    }
}