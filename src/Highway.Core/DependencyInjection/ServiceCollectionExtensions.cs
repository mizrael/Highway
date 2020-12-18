using System;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHighway(this IServiceCollection services, Action<IBusConfigurator> configure = null)
        {
            var stateTypeResolver = new SagaTypeResolver();
            services.AddSingleton<ISagaTypeResolver>(stateTypeResolver);

            services.AddSingleton<ITypesCache, TypesCache>();
            services.AddSingleton<IMessageContextFactory, DefaultMessageContextFactory>();

            services.AddSingleton<IMessageBus, DefaultMessageBus>();

            var builder = new BusConfigurator(services, stateTypeResolver);
            configure?.Invoke(builder);
            
            return services;
        }
    }

}