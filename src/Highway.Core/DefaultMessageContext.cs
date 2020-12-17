using System;

namespace Highway.Core
{
    internal class DefaultMessageContext<TM> : IMessageContext<TM>
        where TM : IMessage
    {
        public DefaultMessageContext(TM message, IPublisher publisher)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public TM Message { get; }
        public IPublisher Publisher { get; }
    }
}