using System.Threading.Tasks;
using Highway.Core;

namespace Highway.Persistence.InMemory
{
    internal class InMemoryPublisher : IPublisher
    {
        public Task PublishAsync<TM>(TM started) where TM : IMessage
        {
            return Task.CompletedTask;
        }
    }
}