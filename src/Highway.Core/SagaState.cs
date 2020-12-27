using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Highway.Core
{
    public abstract class SagaState
    {
        [JsonProperty] //TODO: get rid of Newtonsoft.JSON dependency
        private readonly Queue<IMessage> _outbox = new Queue<IMessage>();

        protected SagaState(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        [JsonIgnore]
        public IReadOnlyCollection<IMessage> Outbox => _outbox;
       
        public void EnqueueMessage(IMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            _outbox.Enqueue(message);
        }

        public async Task<IEnumerable<Exception>> ProcessOutboxAsync(IMessageBus bus, CancellationToken cancellationToken = default)
        {
            var failedMessages = new Queue<IMessage>();
            var exceptions = new List<Exception>();
            
            while (_outbox.Any())
            {
                var message = _outbox.Dequeue();
                try
                {
                    await bus.PublishAsync((dynamic)message, cancellationToken);
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