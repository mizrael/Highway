namespace Highway.Transport.RabbitMQ
{
    public record QueueReferences(string ExchangeName, string QueueName, string DeadLetterExchangeName, string DeadLetterQueue);
}