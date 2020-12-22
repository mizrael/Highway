using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;

namespace Highway.Persistence.Mongo
{
    public class DbContext : IDbContext
    {
        private readonly IMongoDatabase _db;

        private static readonly IBsonSerializer guidSerializer = new GuidSerializer(GuidRepresentation.Standard);

        public DbContext(IMongoDatabase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

            SagaStates = _db.GetCollection<Entities.SagaState>("sagaStates");
        }

        static DbContext()
        {
            BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;

            if (!BsonClassMap.IsClassMapRegistered(typeof(Entities.SagaState)))
                BsonClassMap.RegisterClassMap<Entities.SagaState>(mapper =>
                {
                    mapper.MapIdField(c => c.Id).SetSerializer(guidSerializer);
                    mapper.MapProperty(c => c.Data);
                    mapper.MapProperty(c => c.Type);
                    mapper.MapCreator(c => new Entities.SagaState(c.Id, c.Data, c.Type));
                });
        }
     
        public IMongoCollection<Entities.SagaState> SagaStates { get; }
    }
}