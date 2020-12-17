using System;

namespace Highway.Core
{
    public abstract class Saga<TD>
        where TD : ISagaState
    {
        public abstract Guid GetCorrelationId();
        
        public TD State { get; internal set; }

    }
}