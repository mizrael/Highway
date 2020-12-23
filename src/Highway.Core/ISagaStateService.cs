using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.Persistence;

namespace Highway.Core
{
    public interface ISagaStateService<TS, TD> 
        where TS : Saga<TD> 
        where TD : SagaState
    {
        Task<TD> GetAsync<TM>(IMessageContext<TM> messageContext,
                              ITransaction transaction = null,
                              CancellationToken cancellationToken = default) where TM : IMessage;

        Task SaveAsync(Guid correlationId, TD state, ITransaction transaction = null, CancellationToken cancellationToken = default);
    }
}