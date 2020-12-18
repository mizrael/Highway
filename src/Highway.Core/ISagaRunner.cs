using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public interface ISagaRunner<TS, TD>
        where TS : Saga<TD>
        where TD : ISagaState
    {
        Task RunAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken)
            where TM : IMessage;
    }
}