using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core;
using Highway.Core.Exceptions;
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
                .Returns(Enumerable.Empty<IHandleMessage<DummyEvent>>());
            var messageContextFactory = NSubstitute.Substitute.For<IMessageContextFactory>();

            var sut = new InMemoryPublisher(sp, messageContextFactory);

            await Assert.ThrowsAsync<ConsumerNotFoundException>(async () =>
                await sut.PublishAsync(@event)
            );
        }

        [Fact]
        public async Task Send_should_send_command_only_to_one_consumer()
        {
            var @event = new DummyCommand(Guid.NewGuid());

            var consumer = NSubstitute.Substitute.For<IHandleMessage<DummyCommand>>();
            var invalidConsumers = Enumerable.Repeat(1, 5)
                .Select(i => NSubstitute.Substitute.For<IHandleMessage<DummyCommand>>())
                .ToArray();

            var sp = NSubstitute.Substitute.For<IServiceProvider>();
            sp.GetService(typeof(IEnumerable<IHandleMessage<DummyCommand>>))
                .Returns(invalidConsumers);
            sp.GetService(typeof(IHandleMessage<DummyCommand>))
                .Returns(consumer);
            var messageContextFactory = NSubstitute.Substitute.For<IMessageContextFactory>();

            var sut = new InMemoryPublisher(sp, messageContextFactory);

            await sut.SendAsync(@event);

            foreach (var c in invalidConsumers)
                await c.Received(0).HandleAsync(Arg.Any<IMessageContext<DummyCommand>>(), Arg.Any<CancellationToken>());

            await consumer.Received(1).HandleAsync(Arg.Any<IMessageContext<DummyCommand>>(), Arg.Any<CancellationToken>());
        }
    }

    public record DummyEvent(Guid Id) : IEvent
    {
        public Guid GetCorrelationId() => this.Id;
    }

    public record DummyCommand(Guid Id) : ICommand
    {
        public Guid GetCorrelationId() => this.Id;
    }
}
