using System;

namespace Highway.Persistence.Mongo.Entities
{
    public record SagaState(Guid Id, byte[] Data, string Type, Guid? LockId = null, DateTime? LockTime = null);
}