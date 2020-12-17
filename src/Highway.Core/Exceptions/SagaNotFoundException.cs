using System;

namespace Highway.Core.Exceptions
{
    public class SagaNotFoundException : Exception
    {
        public Type SagaType { get; }

        public SagaNotFoundException(Type sagaType) : base($"unable to create Saga by type")
        {
            SagaType = sagaType;
        }
    }
}