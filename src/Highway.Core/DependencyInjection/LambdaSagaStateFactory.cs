using System;

namespace Highway.Core.DependencyInjection
{
    internal class LambdaSagaStateFactory<TD> : ISagaStateFactory<TD>
        where TD : ISagaState
    {
        private readonly Func<IMessage, TD> _factory;

        public LambdaSagaStateFactory(Func<IMessage, TD> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public TD Create(IMessage message) => _factory(message);
    }
}