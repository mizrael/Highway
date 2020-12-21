using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("UnitTests")]
namespace Highway.Core.DependencyInjection
{
    internal class BusConfigurator : IBusConfigurator
    {
        private readonly ISagaTypeResolver _typeResolver;

        public BusConfigurator(IServiceCollection services, ISagaTypeResolver typeResolver)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        public IBusConfigurator AddConsumer<TC, TM>()
            where TC : class, IHandleMessage<TM>
            where TM : IMessage
        {
            var messageType = typeof(TM);
            if (messageType.IsAssignableTo(typeof(ICommand)) && 
                Services.Any(sd => sd.ServiceType == typeof(IHandleMessage<TM>) ))
                throw new TypeLoadException(
                    $"there is already one handler registered for command type '{messageType.FullName}'");

            Services.AddScoped<IHandleMessage<TM>, TC>();
            return this;
        }

        public ISagaConfigurator<TS, TD> AddSaga<TS, TD>() where TS : Saga<TD> where TD : SagaState
        {
            var sagaType = typeof(TS);
            var sagaStateType = typeof(TD);

            Services.AddScoped<TS>();

            var messageHandlerType = typeof(IHandleMessage<>).GetGenericTypeDefinition();

            var interfaces = sagaType.GetInterfaces();
            foreach(var i in interfaces)
            {   
                if (!i.IsGenericType) 
                    continue;
                
                var openGeneric = i.GetGenericTypeDefinition();
                if (!openGeneric.IsAssignableFrom(messageHandlerType)) 
                    continue;
                
                var messageType = i.GetGenericArguments().First();

                if (messageType.IsAssignableTo(typeof(ICommand)))
                {
                    var commandHandlerType = typeof(IHandleMessage<>).MakeGenericType(messageType);
                    if(Services.Any(sd => sd.ServiceType == commandHandlerType))
                        throw new TypeLoadException(
                            $"there is already one handler registered for command type '{messageType.FullName}'");
                }

                _typeResolver.Register(messageType, (sagaType, sagaStateType));

                Services.AddTransient(i, sagaType);
            }

            Services.AddSingleton(typeof(ISagaRunner<,>).MakeGenericType(sagaType, sagaStateType),
                                  typeof(SagaRunner<,>).MakeGenericType(sagaType, sagaStateType));
                
            Services.AddSingleton(typeof(ISagaFactory<,>).MakeGenericType(sagaType, sagaStateType),
                                typeof(DefaultSagaFactory<,>).MakeGenericType(sagaType, sagaStateType));

            return new SagaConfigurator<TS, TD>(Services);
        }

        public IServiceCollection Services { get; }
    }
}