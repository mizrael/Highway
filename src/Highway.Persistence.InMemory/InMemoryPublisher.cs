using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Highway.Persistence.InMemory.Tests")]
namespace Highway.Persistence.InMemory
{
    internal class InMemoryPublisher : IPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageContextFactory _messageContextFactory;
        
        public InMemoryPublisher(IServiceProvider serviceProvider, IMessageContextFactory messageContextFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _messageContextFactory = messageContextFactory ?? throw new ArgumentNullException(nameof(messageContextFactory));
        }

        public async Task PublishAsync<TE>(TE @event, CancellationToken cancellationToken = default) where TE : IEvent
        {
            var consumers = _serviceProvider.GetServices<IHandleMessage<TE>>();
            if (null == consumers || !consumers.Any())
                throw new ConsumerNotFoundException(typeof(TE));

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

            if (null != exceptions && exceptions.Any())
                throw new AggregateException(exceptions);
        }

        public async Task SendAsync<TC>(TC command, CancellationToken cancellationToken = default) where TC : ICommand
        {
            var context = _messageContextFactory.Create(command);
            var consumer = _serviceProvider.GetService<IHandleMessage<TC>>();
            if(null == consumer)
                throw new ConsumerNotFoundException(typeof(TC));

            await consumer.HandleAsync(context, cancellationToken);
        }
    }
}