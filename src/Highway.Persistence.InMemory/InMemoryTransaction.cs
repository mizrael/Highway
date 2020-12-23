using System.Threading;
using System.Threading.Tasks;
using Highway.Core.Persistence;

namespace Highway.Persistence.InMemory
{
    internal class InMemoryTransaction : ITransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}