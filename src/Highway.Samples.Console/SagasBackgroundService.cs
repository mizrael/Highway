using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Microsoft.Extensions.Hosting;

namespace Highway.Samples.Console
{
    public class SagasBackgroundService : BackgroundService
    {
        private readonly IMessageBus _bus;

        public SagasBackgroundService(IMessageBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var message = new StartDummySaga(Guid.NewGuid());
            await _bus.PublishAsync(message, stoppingToken);
        }
    }
}
