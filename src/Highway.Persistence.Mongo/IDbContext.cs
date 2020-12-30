using MongoDB.Driver;

namespace Highway.Persistence.Mongo
{
    public interface IDbContext
    {
        IMongoCollection<Entities.SagaState> SagaStates { get; }
    }
}