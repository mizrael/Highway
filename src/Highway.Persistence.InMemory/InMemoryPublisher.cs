using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Highway.Persistence.InMemory.Tests")]
namespace Highway.Persistence.InMemory
{
    internal class InMemoryPublisher<TM> : IPublisher<TM>
        where TM : IMessage
    {
        private readonly IServiceProvider _serviceProvider;

        public InMemoryPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task PublishAsync(TM message, CancellationToken cancellationToken = default)
        {
            if (message == null) 
                throw new ArgumentNullException(nameof(message));
            var subscriber = _serviceProvider.GetService<ISubscriber<TM>>() as InMemorySubscriber<TM>;
            if (null == subscriber)
                return;
            await subscriber.ConsumeAsync(message, cancellationToken);
        }
    }
}