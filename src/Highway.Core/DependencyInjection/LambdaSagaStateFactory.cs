using System;

namespace Highway.Core.DependencyInjection
{
    internal class LambdaSagaStateFactory<TD> : ISagaStateFactory<TD>
    {
        private readonly Func<TD> _factory;

        public LambdaSagaStateFactory(Func<TD> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public TD Create() => _factory();
    }
}