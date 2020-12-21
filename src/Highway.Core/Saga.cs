using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public abstract class Saga<TD>
        where TD : ISagaState
    {
        public abstract Guid GetCorrelationId();

        protected Task PublishAsync<TM>(TM message, CancellationToken cancellationToken = default) where TM : IMessage
        {
            //TODO: outbox
        }

        public TD State { get; internal set; }

    }
}