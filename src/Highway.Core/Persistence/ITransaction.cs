using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Persistence
{
    public interface ITransaction
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}