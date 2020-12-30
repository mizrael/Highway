using System;
using MongoDB.Bson;

namespace Highway.Persistence.Mongo.Entities
{
    public record SagaState(ObjectId _id, Guid CorrelationId, string Type, byte[] Data, Guid? LockId = null, DateTime? LockTime = null);
}