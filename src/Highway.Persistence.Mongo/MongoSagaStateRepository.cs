using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Persistence;
using MongoDB.Driver;

namespace Highway.Persistence.Mongo
{
    public class MongoSagaStateRepository : ISagaStateRepository
    {
        private readonly IDbContext _dbContext;
        
        public MongoSagaStateRepository(IDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TD> FindByCorrelationIdAsync<TD>(Guid correlationId, ITransaction transaction = null, CancellationToken cancellationToken = default)
            where TD : SagaState
        {
            var filter = Builders<Entities.SagaState>.Filter.Eq(s => s.Id, correlationId);
            
            var mongoTransaction = transaction as MongoTransaction;

            var cursor = await _dbContext.SagaStates.FindAsync(s => s.Id == correlationId, null, cancellationToken).ConfigureAwait(false);
            var entity = await cursor.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (entity is null)
                return null;

            // can't deserialize a BsonDocument to <TD> so we have to use JSON instead
            var state = System.Text.Json.JsonSerializer.Deserialize<TD>(entity.Data);
            return state;
        }

        public async Task SaveAsync<TD>(Guid correlationId, TD state, ITransaction transaction = null, CancellationToken cancellationToken = default)
            where TD : SagaState
        {
            var json = System.Text.Json.JsonSerializer.Serialize(state);
            
            var stateType = typeof(TD);

            var update = Builders<Entities.SagaState>.Update
                .Set(s => s.Id, correlationId)
                .Set(s => s.Type, stateType.FullName)
                .Set(s => s.Data, json)
                .Inc(s => s.Version, 1);

            var options = new UpdateOptions(){
                IsUpsert = true
            };

            var mongoTransaction = transaction as MongoTransaction;
            
            await _dbContext.SagaStates.UpdateOneAsync(mongoTransaction?.Session, 
                    s => s.Id == correlationId, update, options, 
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}