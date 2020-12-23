using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using Highway.Core.Persistence;
using NSubstitute;
using Xunit;

namespace Highway.Core.Tests
{
    public class SagaStateServiceTests
    {
        [Fact]
        public async Task GetAsync_should_throw_StateCreationException_if_saga_state_cannot_be_build()
        {
            var sagaStateFactory = NSubstitute.Substitute.For<ISagaStateFactory<DummySagaState>>();
            var sagaStateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();
            var publisher = NSubstitute.Substitute.For<IMessageBus>();

            var sut = new SagaStateService<DummySaga, DummySagaState>(sagaStateFactory, sagaStateRepo, publisher);

            var message = new StartDummySaga(Guid.NewGuid());
            var messageContext = NSubstitute.Substitute.For<IMessageContext<StartDummySaga>>();
            messageContext.Message.Returns(message);

            await Assert.ThrowsAsync<StateCreationException>(() =>
                sut.GetAsync(messageContext, CancellationToken.None));
        }

        [Fact]
        public async Task GetAsync_should_throw_StateCreationException_if_message_cannot_start_saga()
        {
            var sagaStateFactory = NSubstitute.Substitute.For<ISagaStateFactory<DummySagaState>>();
            var sagaStateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();
            var publisher = NSubstitute.Substitute.For<IMessageBus>();

            var sut = new SagaStateService<DummySaga, DummySagaState>(sagaStateFactory, sagaStateRepo, publisher);

            var message = new DummySagaStarted(Guid.NewGuid());
            var messageContext = NSubstitute.Substitute.For<IMessageContext<DummySagaStarted>>();
            messageContext.Message.Returns(message);

            var ex = await Assert.ThrowsAsync<StateCreationException>(() =>
                sut.GetAsync(messageContext, CancellationToken.None));
            ex.Message.Should().Contain("saga cannot be started by message");
        }

        [Fact]
        public async Task GetAsync_should_return_state_from_factory_if_message_can_start_saga()
        {
            var expectedState = new DummySagaState(Guid.NewGuid());

            var message = new StartDummySaga(Guid.NewGuid());
            var messageContext = NSubstitute.Substitute.For<IMessageContext<StartDummySaga>>();
            messageContext.Message.Returns(message);

            var sagaStateFactory = NSubstitute.Substitute.For<ISagaStateFactory<DummySagaState>>();
            sagaStateFactory.Create(message)
                .Returns(expectedState);
                
            var sagaStateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();
            var publisher = NSubstitute.Substitute.For<IMessageBus>();

            var sut = new SagaStateService<DummySaga, DummySagaState>(sagaStateFactory, sagaStateRepo, publisher);

            var state = await sut.GetAsync(messageContext, CancellationToken.None);
            state.Should().Be(expectedState);
        }

    }
}