using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Tests
{
    public record DummySagaState(Guid Id) : SagaState;

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
        public virtual async Task HandleAsync(IMessageContext<StartDummySaga> context, CancellationToken cancellationToken = default)
        {
            var started = new DummySagaStarted(context.Message.Id);
            this.Publish(started);
        }
        
        public virtual Task HandleAsync(IMessageContext<DummySagaStarted> context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
