using System;

namespace Highway.Core
{
    public interface IMessage
    {
        Guid Id { get; }
        Guid CorrelationId { get; }
    }
}