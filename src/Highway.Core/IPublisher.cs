using System.Threading.Tasks;

namespace Highway.Core
{
    public interface IPublisher
    {
        Task PublishAsync<TM>(TM started) where TM : IMessage;
    }
}