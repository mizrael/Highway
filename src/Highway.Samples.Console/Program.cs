using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.DependencyInjection;
using Highway.Persistence.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Highway.Samples.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(cfg =>
                {
                    cfg.AddConsole();
                })
                .AddHighway(cfg =>
                {
                    cfg.AddSaga<DummySaga, DummySagaState>()
                        .UseStateFactory(msg => new DummySagaState(msg.GetCorrelationId()))
                        .PersistInMemory();
                });

            var sp = services.BuildServiceProvider();
            var bus = sp.GetRequiredService<IMessageBus>();

            var message = new StartDummySaga(Guid.NewGuid());
            await bus.PublishAsync(message);
        }
    }

    public record DummySagaState(Guid Id) : SagaState
    {
        public static DummySagaState Empty() => new DummySagaState(Guid.Empty);
    }

    public record StartDummySaga(Guid Id) : ICommand
    {
        public Guid GetCorrelationId() => this.Id;
    }

    public record DummySagaStarted(Guid Id) : IEvent
    {
        public Guid GetCorrelationId() => this.Id;
    }

    public class DummySaga :
        Saga<DummySagaState>,
        IStartedBy<StartDummySaga>,
        IHandleMessage<DummySagaStarted>
    {
        private readonly ILogger<DummySaga> _logger;

        public DummySaga(ILogger<DummySaga> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(IMessageContext<StartDummySaga> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"starting saga '{context.Message.GetCorrelationId()}'...");
            
            var started = new DummySagaStarted(context.Message.Id);
            this.Publish(started);
        }

        public async Task HandleAsync(IMessageContext<DummySagaStarted> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"saga '{context.Message.GetCorrelationId()}' started!");
        }
    }
}
