using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Highway.Samples.Console
{
    public class SagasBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SagasBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            var message = new StartDummySaga(Guid.NewGuid());
            await bus.PublishAsync(message, stoppingToken);
        }
    }
}
