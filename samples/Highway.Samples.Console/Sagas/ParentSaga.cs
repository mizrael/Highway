using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Microsoft.Extensions.Logging;

namespace Highway.Samples.Console.Sagas
{
    public class ParentSagaState : SagaState{
        public ParentSagaState(Guid id) : base(id){}
    }

    public record StartParentSaga(Guid Id, Guid CorrelationId) : ICommand { }

    public record ProcessParentSaga(Guid Id, Guid CorrelationId) : ICommand { }

    public record ParentSagaCompleted(Guid Id, Guid CorrelationId) : IEvent { }

    public class ParentSaga :
        Saga<ParentSagaState>,
        IStartedBy<StartParentSaga>,
        IHandleMessage<ProcessParentSaga>,
        IHandleMessage<ChildSagaCompleted>,
        IHandleMessage<ParentSagaCompleted>
    {
        private readonly ILogger<ParentSaga> _logger;

        private readonly Random _random = new Random();

        public ParentSaga(ILogger<ParentSaga> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(IMessageContext<StartParentSaga> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"starting parent saga '{context.Message.CorrelationId}'...");
            
            var processMessage = new ProcessParentSaga(Guid.NewGuid(), context.Message.CorrelationId);
            this.Publish(processMessage);
        }
        
        public async Task HandleAsync(IMessageContext<ProcessParentSaga> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"starting child saga from parent saga '{context.Message.CorrelationId}'...");
            
            var processMessage = new StartChildSaga(Guid.NewGuid(), context.Message.CorrelationId);
            this.Publish(processMessage);
        }

        public async Task HandleAsync(IMessageContext<ChildSagaCompleted> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"child saga completed, finalizing parent saga '{context.Message.CorrelationId}'...");

            await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 5)), cancellationToken);
            
            var started = new ParentSagaCompleted(Guid.NewGuid(), context.Message.CorrelationId);
            this.Publish(started);
        }

        public async Task HandleAsync(IMessageContext<ParentSagaCompleted> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"parent saga '{context.Message.CorrelationId}' completed!");
        }
    }
}
