﻿using FluentAssertions;
using Highway.Core.Exceptions;
using MongoDB.Driver;
using NSubstitute;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using Xunit;

namespace Highway.Persistence.Mongo.Tests.Unit
{
    public class MongoSagaStateRepositoryTests
    {
        [Fact]
        public async Task LockAsync_should_create_and_return_locked_item_if_not_existing()
        {
            var newState = DummyState.New();

            var jsonState = Newtonsoft.Json.JsonConvert.SerializeObject(newState);
            var stateData = Encoding.UTF8.GetBytes(jsonState);

            var entity = new Entities.SagaState(ObjectId.GenerateNewId(), newState.Id, typeof(DummyState).FullName, stateData, Guid.NewGuid(), DateTime.UtcNow);

            var coll = NSubstitute.Substitute.For<IMongoCollection<Entities.SagaState>>();
            coll.FindOneAndUpdateAsync(Arg.Any<FilterDefinition<Entities.SagaState>>(),
                    Arg.Any<UpdateDefinition<Entities.SagaState>>(),
                    Arg.Any<FindOneAndUpdateOptions<Entities.SagaState>>(),
                    Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(entity);

            var dbContext = NSubstitute.Substitute.For<IDbContext>();
            dbContext.SagaStates.Returns(coll);

            var serializer = NSubstitute.Substitute.For<ISagaStateSerializer>();
            serializer.DeserializeAsync<DummyState>(stateData)
                .Returns(newState);

            var options = new MongoSagaStateRepositoryOptions(TimeSpan.FromMinutes(1));
            var sut = new MongoSagaStateRepository(dbContext, serializer, options);

            var (state, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);
            state.Should().NotBeNull();
            state.Id.Should().Be(newState.Id);
            state.Bar.Should().Be(newState.Bar);
            state.Foo.Should().Be(newState.Foo);
        }

        [Fact]
        public async Task UpdateAsync_should_throw_when_update_fails()
        {
            var newState = DummyState.New();

            var coll = NSubstitute.Substitute.For<IMongoCollection<Entities.SagaState>>();
            coll.UpdateOneAsync(Arg.Any<FilterDefinition<Entities.SagaState>>(),
                    Arg.Any<UpdateDefinition<Entities.SagaState>>(),
                    Arg.Any<UpdateOptions>(),
                    Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs((UpdateResult)null);

            var dbContext = NSubstitute.Substitute.For<IDbContext>();
            dbContext.SagaStates.Returns(coll);

            var serializer = NSubstitute.Substitute.For<ISagaStateSerializer>();
            var options = new MongoSagaStateRepositoryOptions(TimeSpan.FromMinutes(1));
            var sut = new MongoSagaStateRepository(dbContext, serializer, options);

            var ex = await Assert.ThrowsAsync<Exception>(async () => await sut.UpdateAsync(newState, Guid.NewGuid(), false, CancellationToken.None));
            ex.Message.Should().Contain("unable to update saga state");
        }

        [Fact]
        public async Task UpdateAsync_should_throw_when_release_lock_fails()
        {
            var newState = DummyState.New();

            var coll = NSubstitute.Substitute.For<IMongoCollection<Entities.SagaState>>();
            coll.UpdateOneAsync(Arg.Any<FilterDefinition<Entities.SagaState>>(),
                    Arg.Any<UpdateDefinition<Entities.SagaState>>(),
                    Arg.Any<UpdateOptions>(),
                    Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs((UpdateResult)null);

            var dbContext = NSubstitute.Substitute.For<IDbContext>();
            dbContext.SagaStates.Returns(coll);

            var serializer = NSubstitute.Substitute.For<ISagaStateSerializer>();
            var options = new MongoSagaStateRepositoryOptions(TimeSpan.FromMinutes(1));
            var sut = new MongoSagaStateRepository(dbContext, serializer, options);

            var ex = await Assert.ThrowsAsync<LockException>(async () => await sut.UpdateAsync(newState, Guid.NewGuid(), true, CancellationToken.None));
            ex.Message.Should().Contain("unable to release lock on saga state");
        }
    }
}
