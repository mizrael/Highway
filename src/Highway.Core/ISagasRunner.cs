using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public interface ISagasRunner
    {
        Task RunAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken = default)
            where TM : IMessage;
    }
}