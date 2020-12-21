using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Persistence;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Highway.Persistence.Mongo
{
    public class MongoSagaStateRepository<TD> : ISagaStateRepository<TD>
        where TD : SagaState
    {
        private readonly IDbContext _dbContext;

        public MongoSagaStateRepository(IDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TD> FindByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
        {
            var cursor = await _dbContext.SagaStates.FindAsync(c => c.Id == correlationId,
                null, cancellationToken);
            var entity = await cursor.FirstOrDefaultAsync(cancellationToken);
            if (entity is null)
                return null;
            
            var payload = BsonSerializer.Deserialize<TD>(entity.Data);
            return payload; 
        }

        public async Task SaveAsync(Guid correlationId, TD state, CancellationToken cancellationToken = default)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(state);
            var payload = BsonDocument.Parse(json);
            
            var entity = new Entities.SagaState(correlationId, payload);
            await _dbContext.SagaStates.InsertOneAsync(entity, options: null, cancellationToken);
        }
    }
}