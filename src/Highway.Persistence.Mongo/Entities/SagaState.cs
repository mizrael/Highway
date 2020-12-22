using MongoDB.Bson;
using System;

namespace Highway.Persistence.Mongo.Entities
{
    public record SagaState(Guid Id, string Data, string Type);
}