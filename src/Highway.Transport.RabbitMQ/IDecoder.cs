using System;

namespace Highway.Transport.RabbitMQ
{
    public interface IDecoder
    {
        object Decode(ReadOnlyMemory<byte> data, Type type);
        T Decode<T>(ReadOnlyMemory<byte> data);
    }
}