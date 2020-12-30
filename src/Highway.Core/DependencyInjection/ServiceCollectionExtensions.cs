using Microsoft.Extensions.DependencyInjection;
using System;

namespace Highway.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHighway(this IServiceCollection services, Action<IBusConfigurator> configure = null)
        {
            var stateTypeResolver = new SagaTypeResolver();

            services.AddSingleton<ISagaTypeResolver>(stateTypeResolver)
                .AddSingleton<ISagasRunner, SagasRunner>()
                .AddSingleton<ITypesCache, TypesCache>()
                .AddSingleton<IMessageContextFactory, DefaultMessageContextFactory>()
                .AddSingleton<IMessageBus, DefaultMessageBus>()
                .AddSingleton<IMessageProcessor, MessageProcessor>();

            var builder = new BusConfigurator(services, stateTypeResolver);
            configure?.Invoke(builder);

            services.AddHostedService<SagasBackgroundService>();

            return services;
        }
    }

}