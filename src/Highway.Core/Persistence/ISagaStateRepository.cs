using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Persistence
{
    public interface ISagaStateRepository
    {
        Task<(TD state, Guid lockId)> LockAsync<TD>(Guid id, TD newEntity = null, CancellationToken cancellationToken = default) where TD : SagaState;
        Task UpdateAsync<TD>(TD state, Guid lockId, bool releaseLock = false, CancellationToken cancellationToken = default) where TD : SagaState;
    }
}