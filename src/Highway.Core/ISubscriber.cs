using System.Threading.Tasks;

namespace Highway.Core
{
    public interface ISubscriber<TM>
        where TM : IMessage
    {
        Task StartAsync();
        Task StopAsync();
    }
}