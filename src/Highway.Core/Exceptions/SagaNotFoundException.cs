using System;

namespace Highway.Core.Exceptions
{
    public class SagaNotFoundException : Exception
    {
        public SagaNotFoundException(string message) : base(message)
        {
        }
    }
}