using System;

namespace Highway.Persistence.Mongo.Entities
{
    public record SagaState(Guid Id, byte[] Data, string Type, int Version);
}