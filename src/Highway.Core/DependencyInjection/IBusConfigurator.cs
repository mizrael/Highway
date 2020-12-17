using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Core.DependencyInjection
{
    public interface IBusConfigurator
    {
        ISagaConfigurator<TS, TD> RegisterSaga<TS, TD>() 
            where TS : Saga<TD>
            where TD : ISagaState;

        IServiceCollection Services { get; }
    }
    
    internal class BusConfigurator : IBusConfigurator
    {
        public IServiceCollection Services { get; }
        
        private readonly ISagaTypeResolver _typeResolver;

        public BusConfigurator(IServiceCollection services, ISagaTypeResolver typeResolver)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        public ISagaConfigurator<TS, TD> RegisterSaga<TS, TD>() where TS : Saga<TD> where TD : ISagaState
        {
            var sagaType = typeof(TS);
            var sagaStateType = typeof(TD);
            
            var handlerType = typeof(IHandleMessage<>).GetGenericTypeDefinition();

            var interfaces = sagaType.GetInterfaces();
            foreach(var i in interfaces)
            {
                if (!i.IsGenericType) 
                    continue;
                
                var openGeneric = i.GetGenericTypeDefinition();
                if (!openGeneric.IsAssignableFrom(handlerType)) 
                    continue;
                
                var messageType = i.GetGenericArguments().First();
                _typeResolver.Register(messageType, (sagaType, sagaStateType));
            }

            Services.AddSingleton(typeof(ISagaFactory<,>).MakeGenericType(sagaType, sagaStateType),
                                    typeof(DefaultSagaFactory<,>).MakeGenericType(sagaType, sagaStateType));
            
            Services.AddScoped<TS>();

            return new SagaConfigurator<TS, TD>(Services);
        }
    }
}