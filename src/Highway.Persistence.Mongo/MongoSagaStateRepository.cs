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
        private readonly ISagaStateSerializer _sagaStateSerializer;

        public MongoSagaStateRepository(IDbContext dbContext, ISagaStateSerializer sagaStateSerializer)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _sagaStateSerializer = sagaStateSerializer ?? throw new ArgumentNullException(nameof(sagaStateSerializer));
        }

        public async Task<TD> FindByCorrelationIdAsync<TD>(Guid correlationId, ITransaction transaction = null, CancellationToken cancellationToken = default)
            where TD : SagaState
        {
            var mongoTransaction = transaction as MongoTransaction;

            var cursor = await _dbContext.SagaStates.FindAsync(mongoTransaction?.Session, s => s.Id == correlationId, null, cancellationToken)
                                                    .ConfigureAwait(false);
            var entity = await cursor.FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (entity is null)
                return null;
            
            var state = await _sagaStateSerializer.DeserializeAsync<TD>(entity.Data, cancellationToken);

            return state;
        }

        public async Task SaveAsync<TD>(Guid correlationId, TD state, ITransaction transaction = null, CancellationToken cancellationToken = default)
            where TD : SagaState
        {
            // can't deserialize a BsonDocument to <TD> so we have to use JSON instead
            var serializedState = await _sagaStateSerializer.SerializeAsync(state, cancellationToken);
            
            var stateType = typeof(TD);

            var update = Builders<Entities.SagaState>.Update
                .Set(s => s.Id, correlationId)
                .Set(s => s.Type, stateType.FullName)
                .Set(s => s.Data, serializedState)
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