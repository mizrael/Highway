﻿using System;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.DependencyInjection;
using Highway.Persistence.InMemory;
using Highway.Persistence.Mongo;
using Highway.Samples.Console.Sagas;
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
            var message = new StartParentSaga(Guid.NewGuid(), Guid.NewGuid());

            await Task.WhenAll(new[]
            {
                host.RunAsync(),
                bus.PublishAsync(message)
            });
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
                        var mongoCfg = new MongoConfiguration(mongoSection["ConnectionString"], 
                                                              mongoSection["DbName"],
                                                            MongoSagaStateRepositoryOptions.Default);

                        var rabbitSection = hostContext.Configuration.GetSection("Rabbit");
                        var rabbitCfg = new RabbitConfiguration(rabbitSection["HostName"], 
                            rabbitSection["UserName"],
                            rabbitSection["Password"]);

                        cfg.AddSaga<ParentSaga, ParentSagaState>()
                            .UseStateFactory(msg => new ParentSagaState(msg.CorrelationId))
                            .UseRabbitMQTransport(rabbitCfg)
                            .UseMongoPersistence(mongoCfg);

                        cfg.AddSaga<ChildSaga, ChildSagaState>()
                            .UseStateFactory(msg => new ChildSagaState(msg.CorrelationId))
                            .UseRabbitMQTransport(rabbitCfg)
                            .UseMongoPersistence(mongoCfg);
                    });
            });
    }
}
