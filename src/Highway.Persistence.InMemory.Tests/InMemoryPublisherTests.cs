using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Highway.Persistence.InMemory.Tests
{
    public class InMemoryPublisherTests
    {
        [Fact]
        public async Task Publish_should_broadcast_event_to_all_registered_consumers()
        {
            var @event = new DummyEvent(Guid.NewGuid());

            var consumers = Enumerable.Repeat(1, 5)
                .Select(i => NSubstitute.Substitute.For<IHandleMessage<DummyEvent>>())
                .ToArray();

            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            sp.GetService(typeof(IEnumerable<IHandleMessage<DummyEvent>>))
                .Returns(consumers);
            sp.GetServices<IHandleMessage<DummyEvent>>()
                .Returns(consumers);
            var messageContextFactory = NSubstitute.Substitute.For<IMessageContextFactory>();

            var sut = new InMemoryPublisher(sp, messageContextFactory);

            await sut.PublishAsync(@event);

            foreach (var c in consumers)
                await c.Received(1).HandleAsync(Arg.Any<IMessageContext<DummyEvent>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Publish_should_throw_ConsumersNotFoundException_when_no_consumers_registered()
        {
            var @event = new DummyEvent(Guid.NewGuid());

            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            sp.GetService(typeof(IEnumerable<IHandleMessage<DummyEvent>>))
                .Returns(null);
            var messageContextFactory = NSubstitute.Substitute.For<IMessageContextFactory>();

            var sut = new InMemoryPublisher(sp, messageContextFactory);

            await Assert.ThrowsAsync<ConsumersNotFoundException>(async () =>
                await sut.PublishAsync(@event)
            );
        }
    }

    public record DummyEvent(Guid Id) : IMessage
    {
        public Guid GetCorrelationId() => this.Id;
    }
}
