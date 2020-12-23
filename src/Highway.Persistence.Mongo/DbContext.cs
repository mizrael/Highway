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
                    mapper.MapProperty(c => c.Version);
                    mapper.MapCreator(s => new Entities.SagaState(s.Id, s.Data, s.Type, s.Version));
                });
        }
     
        public IMongoCollection<Entities.SagaState> SagaStates { get; }
    }
}