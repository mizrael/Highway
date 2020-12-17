using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public interface IPublisher
    {
        Task PublishAsync<TE>(TE @event, CancellationToken cancellationToken = default) where TE : IEvent;
        Task SendAsync<TC>(TC command, CancellationToken cancellationToken = default) where TC : ICommand;
    }
}