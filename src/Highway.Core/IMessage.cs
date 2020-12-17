using System;

namespace Highway.Core
{
    public interface IMessage
    {
        Guid GetCorrelationId();
    }
}