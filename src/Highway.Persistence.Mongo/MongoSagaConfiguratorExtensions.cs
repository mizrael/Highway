using Highway.Core;
using Highway.Core.DependencyInjection;
using Highway.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Highway.Persistence.Mongo
{
    public record MongoConfiguration(string ConnectionString, string DbName);

    public static class MongoSagaConfiguratorExtensions
    {
        public static ISagaConfigurator<TS, TD> UseMongoPersistence<TS, TD>(
            this ISagaConfigurator<TS, TD> sagaConfigurator, MongoConfiguration config)
            where TS : Saga<TD>
            where TD : SagaState
        {
            sagaConfigurator.Services.AddSingleton(ctx => new MongoClient(connectionString: config.ConnectionString))
                .AddSingleton(ctx =>
                {
                    var client = ctx.GetRequiredService<MongoClient>();
                    var database = client.GetDatabase(config.DbName);
                    return database;
                })
                .AddSingleton<ISagaStateSerializer, JsonSagaStateSerializer>()
                .AddSingleton<IDbContext, DbContext>()
                .AddSingleton<IUnitOfWork, MongoUnitOfWork>()
                .AddSingleton<ISagaStateRepository, MongoSagaStateRepository>();
            return sagaConfigurator;
        }
    }
}