using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Microsoft.Extensions.Logging;

namespace Highway.Samples.Console
{

    public class DummySagaState : SagaState{
        public DummySagaState(Guid id) : base(id){}
    }

    public record StartDummySaga(Guid Id, Guid CorrelationId) : ICommand
    {
    }

    public record DummySagaStarted(Guid Id, Guid CorrelationId) : IEvent
    {
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
            _logger.LogInformation($"starting saga '{context.Message.CorrelationId}'...");
            
            var started = new DummySagaStarted(context.Message.Id, context.Message.CorrelationId);
            this.Publish(started);
        }

        public async Task HandleAsync(IMessageContext<DummySagaStarted> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"saga '{context.Message.CorrelationId}' started!");
        }
    }
}
