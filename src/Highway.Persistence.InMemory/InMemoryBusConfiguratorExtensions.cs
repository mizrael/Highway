using Highway.Core;
using Highway.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Persistence.InMemory
{
    public static class InMemoryBusConfiguratorExtensions
    {
        public static IBusConfigurator InMemory(this IBusConfigurator busConfigurator)
        {
            busConfigurator.Services.AddSingleton<IPublisher, InMemoryPublisher>();
            return busConfigurator;
        }
    }
}