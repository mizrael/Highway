using System;
using Highway.Core;
using RabbitMQ.Client;

namespace Highway.Transport.RabbitMQ
{
    public interface IMessageResolver
    {
        IMessage Resolve(IBasicProperties basicProperties, ReadOnlyMemory<byte> body);
    }
}