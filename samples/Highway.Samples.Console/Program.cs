using System;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.DependencyInjection;
using Highway.Persistence.InMemory;
using Highway.Persistence.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Highway.Samples.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);
            var host = hostBuilder.Build();

            var bus = host.Services.GetRequiredService<IMessageBus>();
            var message = new StartDummySaga(Guid.NewGuid());
            await bus.PublishAsync(message);

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(cfg =>
                    {
                        cfg.AddConsole();
                    })
                    .AddHighway(cfg =>
                    {
                        var mongoSection = hostContext.Configuration.GetSection("Mongo");
                        var mongoCfg = new MongoConfiguration(mongoSection["ConnectionString"], mongoSection["DbName"]);

                        cfg.AddSaga<DummySaga, DummySagaState>()
                            .UseStateFactory(msg => new DummySagaState(msg.GetCorrelationId()))
                            .UseInMemoryTransport()
                            .UseMongoPersistence(mongoCfg);
                    });
            });
    }
}
