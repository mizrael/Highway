using System;

namespace Highway.Core.Exceptions
{
    public class ConsumersNotFoundException : Exception
    {
        public Type MessageType { get; }

        public ConsumersNotFoundException(Type messageType) : base($"no consumers found for message messageType '{messageType.FullName}'")
        {
            MessageType = messageType;
        }
    }
}