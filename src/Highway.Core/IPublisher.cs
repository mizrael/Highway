using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public interface IPublisher<in TM>
        where TM : IMessage
    {
        Task PublishAsync(TM message, CancellationToken cancellationToken = default);
    }
}