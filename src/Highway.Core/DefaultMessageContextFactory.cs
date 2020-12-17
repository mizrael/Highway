using System;

namespace Highway.Core
{
    internal class DefaultMessageContextFactory : IMessageContextFactory
    {
        private readonly IPublisher _publisher;

        public DefaultMessageContextFactory(IPublisher publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public IMessageContext<TM> Create<TM>(TM message) where TM : IMessage
        {
            return new DefaultMessageContext<TM>(message, _publisher);
        }
    }
}