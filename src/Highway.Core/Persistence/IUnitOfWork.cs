using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Persistence
{
    public interface IUnitOfWork
    {
        ISagaStateRepository SagaStatesRepository { get; }

        Task<ITransaction> StartTransactionAsync(CancellationToken cancellationToken = default);
    }
}