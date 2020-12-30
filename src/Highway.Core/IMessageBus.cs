using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public interface IMessageBus
    {
        Task PublishAsync<TM>(TM message, CancellationToken cancellationToken = default) where TM : IMessage;
    }

    internal class DefaultMessageBus : IMessageBus
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultMessageBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task PublishAsync<TM>(TM message, CancellationToken cancellationToken = default) where TM : IMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            var publisher = _serviceProvider.GetService<IPublisher<TM>>();
            if (null == publisher)
                return;
            await publisher.PublishAsync(message, cancellationToken)
                            .ConfigureAwait(false);
        }
    }
}