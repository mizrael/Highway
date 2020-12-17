using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core.Tests
{
    public record DummySagaState(Guid Id) : ISagaState
    {
        public static DummySagaState Empty() => new DummySagaState(Guid.Empty);
    }

    public record DummySagaStarter(Guid Id) : IMessage
    {
        public Guid GetCorrelationId() => this.Id;
    }

    public record DummySagaStarted(Guid Id) : IMessage
    {
        public Guid GetCorrelationId() => this.Id;
    }

    public class DummySaga : 
        Saga<DummySagaState>,
        IStartedBy<DummySagaStarter>,
        IHandleMessage<DummySagaStarted>
    {
        public override Guid GetCorrelationId() => this.State.Id;

        public virtual async Task HandleAsync(IMessageContext<DummySagaStarter> context, CancellationToken cancellationToken = default)
        {
            var started = new DummySagaStarted(context.Message.Id);
            await context.Publisher.PublishAsync(started);
        }
        
        public virtual Task HandleAsync(IMessageContext<DummySagaStarted> context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
