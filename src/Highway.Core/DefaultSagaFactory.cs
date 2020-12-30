using Microsoft.Extensions.DependencyInjection;
using System;

namespace Highway.Core
{
    internal class DefaultSagaFactory<TS, TD> : ISagaFactory<TS, TD>
        where TD : SagaState
        where TS : Saga<TD>
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultSagaFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public TS Create(TD state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            using var scope = _serviceProvider.CreateScope();
            var saga = scope.ServiceProvider.GetRequiredService<TS>();
            saga.State = state;
            return saga;
        }
    }
}