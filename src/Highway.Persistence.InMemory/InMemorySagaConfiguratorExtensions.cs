using Highway.Core;
using Highway.Core.DependencyInjection;
using Highway.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Persistence.InMemory
{
    public static class InMemorySagaConfiguratorExtensions
    {
        public static ISagaConfigurator<TS, TD> PersistInMemory<TS, TD>(this ISagaConfigurator<TS, TD> sagaConfigurator)
            where TS : Saga<TD>
            where TD : ISagaState
        {
            var sagaStateType = typeof(TD);
            var intrT = typeof(ISagaStateRepository<>).MakeGenericType(sagaStateType);
            var implT = typeof(InMemorySagaStateRepository<>).MakeGenericType(sagaStateType);
            sagaConfigurator.Services.AddSingleton(intrT, implT);

            return sagaConfigurator;
        }
    }
}