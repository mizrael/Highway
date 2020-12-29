using Highway.Core;

namespace Highway.Transport.RabbitMQ
{
    public interface IQueueReferenceFactory
    {
        QueueReferences Create<TM>() where TM : IMessage;
    }
}