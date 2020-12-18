using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;

namespace Highway.Persistence.InMemory
{
    internal class InMemorySubscriber<TM> : ISubscriber<TM>
        where TM : IMessage
    {
        private readonly IMessageProcessor _messageProcessor;
        
        public InMemorySubscriber(IMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        public async Task ConsumeAsync(TM message, CancellationToken cancellationToken = default)
        {
            await _messageProcessor.ProcessAsync(message, cancellationToken);
        }
    }
}