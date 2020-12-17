using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Persistence.InMemory
{
    public class InMemoryPublisher : IPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageContextFactory _messageContextFactory;
        
        public InMemoryPublisher(IServiceProvider serviceProvider, IMessageContextFactory messageContextFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _messageContextFactory = messageContextFactory ?? throw new ArgumentNullException(nameof(messageContextFactory));
        }

        public async Task PublishAsync<TM>(TM @event, CancellationToken cancellationToken = default) where TM : IMessage
        {
            var consumers = _serviceProvider.GetServices<IHandleMessage<TM>>();
            if (null == consumers || !consumers.Any())
                throw new ConsumersNotFoundException(typeof(TM));

            IList<Exception> exceptions = null;
            
            var context = _messageContextFactory.Create(@event);
            foreach (var c in consumers)
            {
                try
                {
                    await c.HandleAsync(context, cancellationToken);
                }
                catch (Exception e)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(e);
                }
            }
        }

        public Task SendAsync<TM>(TM command, CancellationToken cancellationToken = default) where TM : IMessage
        {
            return Task.CompletedTask;
        }
    }
}