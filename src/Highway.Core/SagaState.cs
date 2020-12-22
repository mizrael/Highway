using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public abstract record SagaState
    {
        private readonly Queue<IMessage> _outbox = new Queue<IMessage>();

        public IReadOnlyCollection<IMessage> Outbox => _outbox;
        
        public void EnqueueMessage(IMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            _outbox.Enqueue(message);
        }

        public async Task<IEnumerable<Exception>> ProcessOutboxAsync(IMessageBus _publisher, CancellationToken cancellationToken = default)
        {
            var failedMessages = new Queue<IMessage>();
            var exceptions = new List<Exception>();
            
            while (_outbox.Any())
            {
                var message = _outbox.Dequeue();
                try
                {
                    await _publisher.PublishAsync((dynamic)message, cancellationToken);
                }
                catch (Exception e)
                {
                    failedMessages.Enqueue(message);
                    exceptions.Add(e);
                }
            }
            
            while (failedMessages.Any())
            {
                var failed = failedMessages.Dequeue();
                _outbox.Enqueue(failed);
            }

            return exceptions;
        }
    }
}