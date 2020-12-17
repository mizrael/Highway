using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public interface IPublisher
    {
        Task PublishAsync<TM>(TM @event, CancellationToken cancellationToken = default) where TM : IMessage;
        Task SendAsync<TC>(TC command, CancellationToken cancellationToken = default) where TC : ICommand;
    }
}