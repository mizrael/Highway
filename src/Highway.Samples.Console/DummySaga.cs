using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Microsoft.Extensions.Logging;

namespace Highway.Samples.Console
{

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
