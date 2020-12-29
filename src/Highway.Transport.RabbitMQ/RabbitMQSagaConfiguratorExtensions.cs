using System;
using System.Linq;
using System.Threading.Channels;
using Highway.Core;
using Highway.Core.DependencyInjection;
using Highway.Core.Persistence;
using Highway.Transport.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Highway.Persistence.InMemory
{
    public record RabbitConfiguration(string HostName, string UserName, string Password);
    
    public static class RabbitMQSagaConfiguratorExtensions
    {
        private static bool _initialized = false;
        
        public static ISagaConfigurator<TS, TD> UseRabbitMQTransport<TS, TD>(this ISagaConfigurator<TS, TD> sagaConfigurator, 
            RabbitConfiguration config)
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
                                                    typeof(RabbitPublisher<>).MakeGenericType(messageType));

                sagaConfigurator.Services.AddSingleton(typeof(ISubscriber<>).MakeGenericType(messageType),
                                                    typeof(RabbitSubscriber<>).MakeGenericType(messageType));
            }

            //TODO: this won't work when multiple sagas are registered
            sagaConfigurator.Services.AddSingleton<IMessageResolver>(ctx =>
            {
                var decoder = ctx.GetRequiredService<IDecoder>();
                var assemblies = new[]
                {
                    typeof(TS).Assembly
                };
                return new MessageResolver(decoder, assemblies);
            });

            if (!_initialized)
            {
                var encoder = new JsonEncoder();
                sagaConfigurator.Services.AddSingleton<IEncoder>(encoder);
                sagaConfigurator.Services.AddSingleton<IDecoder>(encoder);
                
                sagaConfigurator.Services.AddSingleton<IQueueReferenceFactory, QueueReferenceFactory>();

                sagaConfigurator.Services.AddSingleton<IConnectionFactory>(ctx =>
                {
                    var connectionFactory = new ConnectionFactory()
                    {
                        HostName = config.HostName,
                        UserName = config.UserName,
                        Password = config.Password,
                        Port = AmqpTcpEndpoint.UseDefaultPort,
                        DispatchConsumersAsync = true
                    };
                    return connectionFactory;
                });

                sagaConfigurator.Services.AddSingleton<IBusConnection, RabbitPersistentConnection>();
                
                _initialized = true;
            }

            return sagaConfigurator;
        }
    }
}