using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Core
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISagasRunner _sagasRunner;
        private readonly IMessageContextFactory _messageContextFactory;

        public MessageProcessor(IServiceProvider serviceProvider, 
            ISagasRunner sagasRunner, IMessageContextFactory messageContextFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _sagasRunner = sagasRunner ?? throw new ArgumentNullException(nameof(sagasRunner));
            _messageContextFactory = messageContextFactory ?? throw new ArgumentNullException(nameof(messageContextFactory));
        }

        public async Task ProcessAsync<TM>(TM message, CancellationToken cancellationToken = default)
            where TM : IMessage
        {
            if (message == null) 
                throw new ArgumentNullException(nameof(message));

            var messageContext = _messageContextFactory.Create(message);

            await _sagasRunner.RunAsync(messageContext, cancellationToken);

            await RunConsumersAsync(messageContext, cancellationToken);
        }

        private async Task RunConsumersAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken) where TM : IMessage
        {
            var consumers = _serviceProvider.GetServices<IHandleMessage<TM>>();

            IList<Exception> exceptions = null;
            
            foreach (var c in consumers)
            {
                try
                {
                    await c.HandleAsync(messageContext, cancellationToken);
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

    }
}