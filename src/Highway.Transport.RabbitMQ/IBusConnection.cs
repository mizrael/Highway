using RabbitMQ.Client;

namespace Highway.Transport.RabbitMQ
{
    public interface IBusConnection
    {
        bool IsConnected { get; }

        IModel CreateChannel();
    }
}