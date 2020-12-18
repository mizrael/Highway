using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Tests
{
    public record DummySagaState(Guid Id) : ISagaState
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
        private readonly IMessageBus _publisher;

        public DummySaga(IMessageBus publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public override Guid GetCorrelationId() => this.State.Id;

        public virtual async Task HandleAsync(IMessageContext<StartDummySaga> context, CancellationToken cancellationToken = default)
        {
            var started = new DummySagaStarted(context.Message.Id);
            await _publisher.PublishAsync(started, cancellationToken);
        }
        
        public virtual Task HandleAsync(IMessageContext<DummySagaStarted> context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
