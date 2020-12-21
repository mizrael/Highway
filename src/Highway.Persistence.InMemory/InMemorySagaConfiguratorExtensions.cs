using System.Linq;
using Highway.Core;
using Highway.Core.DependencyInjection;
using Highway.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Persistence.InMemory
{
    public static class InMemorySagaConfiguratorExtensions
    {
        public static ISagaConfigurator<TS, TD> UseInMemoryPersistence<TS, TD>(this ISagaConfigurator<TS, TD> sagaConfigurator)
            where TS : Saga<TD>
            where TD : SagaState
        {
            var sagaStateType = typeof(TD);

            sagaConfigurator.Services.AddSingleton(typeof(ISagaStateRepository<>).MakeGenericType(sagaStateType),
                typeof(InMemorySagaStateRepository<>).MakeGenericType(sagaStateType));
            return sagaConfigurator;
        }

        public static ISagaConfigurator<TS, TD> UseInMemoryTransport<TS, TD>(this ISagaConfigurator<TS, TD> sagaConfigurator)
            where TS : Saga<TD>
            where TD : SagaState
        {
            var sagaType = typeof(TS);
            var messageHandlerType = typeof(IHandleMessage<>).GetGenericTypeDefinition();
            var interfaces = sagaType.GetInterfaces();
            foreach (var i in interfaces)
            {
                if (!i.IsGenericType)
                    continue;

                var openGeneric = i.GetGenericTypeDefinition();
                if (!openGeneric.IsAssignableFrom(messageHandlerType))
                    continue;

                var messageType = i.GetGenericArguments().First();

                sagaConfigurator.Services.AddSingleton(typeof(IPublisher<>).MakeGenericType(messageType), 
                                                       typeof(InMemoryPublisher<>).MakeGenericType(messageType));

                sagaConfigurator.Services.AddSingleton(typeof(ISubscriber<>).MakeGenericType(messageType),
                                                       typeof(InMemorySubscriber<>).MakeGenericType(messageType));
            }

            return sagaConfigurator;
        }
    }
}