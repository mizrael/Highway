using System.Threading.Tasks;
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
        static Task Main(string[] args) =>
            CreateHostBuilder(args).Build().RunAsync();

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) => {
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
                    }).AddHostedService<SagasBackgroundService>();
        });
    }
}
