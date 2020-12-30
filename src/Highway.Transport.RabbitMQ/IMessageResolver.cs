using Highway.Core;
using RabbitMQ.Client;
using System;

namespace Highway.Transport.RabbitMQ
{
    public interface IMessageResolver
    {
        TM Resolve<TM>(IBasicProperties basicProperties, ReadOnlyMemory<byte> body) where TM : IMessage;
    }
}