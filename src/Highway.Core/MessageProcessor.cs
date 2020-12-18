using System;
using System.Threading;
using System.Threading.Tasks;

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
        }
    }
}