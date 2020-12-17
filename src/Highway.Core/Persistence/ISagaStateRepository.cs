using System;
using System.Threading.Tasks;

namespace Highway.Core.Persistence
{
    public interface ISagaStateRepository<TD>
        where TD : ISagaState
    {
        Task<TD> FindByCorrelationIdAsync(Guid correlationId);
        Task SaveAsync(Guid correlationId, TD state);
    }
}