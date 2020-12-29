using System;
using Highway.Core;
using RabbitMQ.Client;

namespace Highway.Transport.RabbitMQ
{
    public interface IMessageResolver
    {
        TM Resolve<TM>(IBasicProperties basicProperties, ReadOnlyMemory<byte> body) where TM : IMessage;
    }
}