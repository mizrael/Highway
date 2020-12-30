﻿using FluentAssertions;
using Highway.Core.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Highway.Persistence.InMemory.Tests.Unit
{
    public class InMemorySagaStateRepositoryTests
    {
        [Fact]
        public async Task LockAsync_should_create_and_return_locked_item_if_not_existing()
        {
            var sut = new InMemorySagaStateRepository();

            var newState = DummyState.New();
            var (state, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);
            state.Should().NotBeNull();
            state.Id.Should().Be(newState.Id);
            state.Bar.Should().Be(newState.Bar);
            state.Foo.Should().Be(newState.Foo);
        }

        [Fact]
        public async Task LockAsync_should_fail_if_item_already_locked()
        {
            var sut = new InMemorySagaStateRepository();

            var newState = DummyState.New();

            var (lockedState, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);
            lockedState.Should().NotBeNull();

            await Assert.ThrowsAsync<LockException>(async () => await sut.LockAsync(newState.Id, newState, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateAsync_should_release_lock()
        {
            var sut = new InMemorySagaStateRepository();

            var newState = DummyState.New();
            var (lockedState, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);

            var updatedItem = new DummyState(lockedState.Id, "dolor amet", 71);

            await sut.UpdateAsync(updatedItem, lockId, true, CancellationToken.None);
        }

        [Fact]
        public async Task UpdateAsync_should_fail_if_item_not_locked()
        {
            var sut = new InMemorySagaStateRepository();

            var newState = DummyState.New();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.UpdateAsync(newState, Guid.NewGuid(), true, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateAsync_should_fail_if_item_not_locked_anymore()
        {
            var sut = new InMemorySagaStateRepository();

            var newState = DummyState.New();
            var (lockedState, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);

            var updatedItem = new DummyState(lockedState.Id, "dolor amet", 71);

            await sut.UpdateAsync(updatedItem, lockId, true, CancellationToken.None);

            await Assert.ThrowsAsync<LockException>(async () => await sut.UpdateAsync(updatedItem, lockId, true, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateAsync_should_fail_if_item_locked_by_somebody_else()
        {
            var sut = new InMemorySagaStateRepository();

            var newState = DummyState.New();
            var (lockedState, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);

            var updatedItem = new DummyState(lockedState.Id, "dolor amet", 71);

            await Assert.ThrowsAsync<LockException>(async () => await sut.UpdateAsync(updatedItem, Guid.NewGuid(), true, CancellationToken.None));
        }
    }
}
