using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Highway.Core.Tests.Unit
{
    public class SagaStateTests
    {
        [Fact]
        public void EnqueueMessage_should_enqueue_message()
        {
            var state = new DummySagaState(Guid.NewGuid());
            state.Outbox.Should().BeEmpty();
            
            var msg = new StartDummySaga(Guid.NewGuid());
            state.EnqueueMessage(msg);
            state.Outbox.Should().HaveCount(1)
                .And.Contain(m => m.Id == msg.Id);
        }

        [Fact]
        public void EnqueueMessage_should_not_enqueue_the_same_message_more_than_once()
        {
            var state = new DummySagaState(Guid.NewGuid());
            state.Outbox.Should().BeEmpty();

            var msg = new StartDummySaga(Guid.NewGuid());
            state.EnqueueMessage(msg);

            var ex = Assert.Throws<ArgumentException>(() => state.EnqueueMessage(msg));
            ex.Message.Should().Contain($"message '{msg.Id}' was already enqueued");
        }

        [Fact]
        public async Task ProcessOutbox_should_publish_all_messaged()
        {
            var state = new DummySagaState(Guid.NewGuid());
            
            var messages = Enumerable.Repeat(1, 5)
                .Select(i => new StartDummySaga(Guid.NewGuid()))
                .ToArray();
            foreach(var msg in messages)
                state.EnqueueMessage(msg);

            var bus = NSubstitute.Substitute.For<IMessageBus>();
            await state.ProcessOutboxAsync(bus, CancellationToken.None);

            foreach (var msg in messages)
                await bus.Received()
                    .PublishAsync(msg, CancellationToken.None);

            state.Outbox.Should().BeEmpty();
            state.ProcessedMessages.Should().HaveCount(messages.Length);
        }

        [Fact]
        public async Task ProcessOutbox_should_reenqueue_failed_messages()
        {
            var state = new DummySagaState(Guid.NewGuid());
            
            var messages = Enumerable.Repeat(1, 3)
                .Select(i => new StartDummySaga(Guid.NewGuid()))
                .ToArray();
            foreach (var msg in messages)
                state.EnqueueMessage(msg);

            var bus = NSubstitute.Substitute.For<IMessageBus>();
            
            var failedMessage = Enumerable.Repeat(1, 5)
                .Select(i => new StartDummySaga(Guid.NewGuid()))
                .ToArray();
            foreach (var msg in failedMessage)
            {
                state.EnqueueMessage(msg);
                bus.When(b => b.PublishAsync(msg, CancellationToken.None))
                    .Throw(new Exception(msg.Id.ToString()));
            }

            state.Outbox.Should().HaveCount(failedMessage.Length + messages.Length);

            await state.ProcessOutboxAsync(bus, CancellationToken.None);

            foreach (var msg in messages)
                await bus.Received()
                    .PublishAsync(msg, CancellationToken.None);

            state.Outbox.Should().HaveCount(failedMessage.Length);

            state.ProcessedMessages.Should().HaveCount(messages.Length);
        }
    }
}
