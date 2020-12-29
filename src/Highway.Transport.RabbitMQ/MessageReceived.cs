using System;
using Highway.Core;

namespace Highway.Transport.RabbitMQ
{
    public class MessageReceived : EventArgs
    {
        public MessageReceived(IMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public IMessage Message { get; }
    }
}