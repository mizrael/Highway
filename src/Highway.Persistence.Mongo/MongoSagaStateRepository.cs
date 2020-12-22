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

            // can't deserialize a BsonDocument to <TD> so we have to use JSON instead
            var state = System.Text.Json.JsonSerializer.Deserialize<TD>(entity.Data);
            return state;
        }

        public async Task SaveAsync(Guid correlationId, TD state, CancellationToken cancellationToken = default)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(state);
            
            var stateType = typeof(TD);

            var update = Builders<Entities.SagaState>.Update
                .Set(s => s.Id, correlationId)
                .Set(s => s.Type, stateType.FullName)
                .Set(s => s.Data, json);

            var options = new UpdateOptions(){
                IsUpsert = true
            };
            await _dbContext.SagaStates.UpdateOneAsync(s => s.Id == correlationId, update, options, cancellationToken);
        }
    }
}