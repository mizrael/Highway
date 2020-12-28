using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Highway.Persistence.Mongo.Tests.Integration
{
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
            var (state, lockId) = await sut.LockAsync(newState.Id, newState, CancellationToken.None);
            state.Should().NotBeNull();
            state.Id.Should().Be(newState.Id);
            state.Bar.Should().Be(newState.Bar);
            state.Foo.Should().Be(newState.Foo);
        }
    }
}
