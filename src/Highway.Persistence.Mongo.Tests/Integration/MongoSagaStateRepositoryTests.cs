﻿using FluentAssertions;
using Highway.Persistence.Mongo.Tests.Unit;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.Exceptions;
using Xunit;

namespace Highway.Persistence.Mongo.Tests.Integration
{
    [Category("Integration")]
    [Trait("Category", "Integration")]
    public class MongoSagaStateRepositoryTests : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;

        public MongoSagaStateRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task LockAsync_should_create_and_return_locked_item_if_not_existing()
        {
            var serializer = new JsonSagaStateSerializer();

            var options = new MongoSagaStateRepositoryOptions(TimeSpan.FromMinutes(1));
            var sut = new MongoSagaStateRepository(_fixture.DbContext, serializer, options);

            var newState = DummyState.New();
            newState.EnqueueMessage(DummyMessage.New());
            newState.EnqueueMessage(DummyMessage.New());
            newState.EnqueueMessage(DummyMessage.New());

            var (state, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);
            state.Should().NotBeNull();
            state.Id.Should().Be(newState.Id);
            state.Bar.Should().Be(newState.Bar);
            state.Foo.Should().Be(newState.Foo);
            state.Outbox.Should().HaveCount(3);
        }
        
        [Fact]
        public async Task LockAsync_should_throw_if_item_locked()
        {
            var serializer = new JsonSagaStateSerializer();

            var options = new MongoSagaStateRepositoryOptions(TimeSpan.FromMinutes(1));
            var sut = new MongoSagaStateRepository(_fixture.DbContext, serializer, options);

            var newState = DummyState.New();

            var (state, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);

            var ex = await Assert.ThrowsAsync<LockException>(async () => await sut.LockAsync(newState.Id, newState, CancellationToken.None));
            ex.Message.Should().Contain($"saga state '{state.Id}' is already locked");
        }

        [Fact]
        public async Task LockAsync_should_allow_different_saga_state_types_to_share_the_correlation_id()
        {
            var serializer = new JsonSagaStateSerializer();

            var options = new MongoSagaStateRepositoryOptions(TimeSpan.FromMinutes(1));
            var sut = new MongoSagaStateRepository(_fixture.DbContext, serializer, options);

            var newState = DummyState.New();
            var (state, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);

            var newState2 = new DummyState2(state.Id);
            newState2.Id.Should().Be(newState.Id);
            
            var (state2, lockId2) = await sut.LockAsync(newState2.Id, newState2, CancellationToken.None);
            state2.Should().NotBeNull();
            state2.Id.Should().Be(newState.Id);
        }

    }
}
