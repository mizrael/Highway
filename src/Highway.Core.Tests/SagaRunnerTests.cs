using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using Highway.Core.Persistence;
using NSubstitute;
using Xunit;

namespace Highway.Core.Tests
{
    public class SagaRunnerTests 
    {
        [Fact]
        public async Task RunAsync_should_throw_StateCreationException_if_saga_state_cannot_be_build()
        {
            var sagaFactory = NSubstitute.Substitute.For<ISagaFactory<DummySaga, DummySagaState>>();
            var sagaStateFactory = NSubstitute.Substitute.For<ISagaStateFactory<DummySagaState>>();
            var sagaStateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();
            
            var sut = new SagaRunner<DummySaga, DummySagaState>(sagaFactory, sagaStateFactory, sagaStateRepo);

            var message = new StartDummySaga(Guid.NewGuid());
            var messageContext = NSubstitute.Substitute.For<IMessageContext<StartDummySaga>>();
            messageContext.Message.Returns(message);

            await Assert.ThrowsAsync<StateCreationException>(() => sut.RunAsync(messageContext, CancellationToken.None));
        }

        [Fact]
        public async Task RunAsync_should_throw_SagaNotFoundException_if_saga_cannot_be_build()
        {
            var message = new StartDummySaga(Guid.NewGuid()); 
            
            var sagaFactory = NSubstitute.Substitute.For<ISagaFactory<DummySaga, DummySagaState>>();
            var sagaStateFactory = NSubstitute.Substitute.For<ISagaStateFactory<DummySagaState>>();
            var sagaStateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();

            var state = new DummySagaState(message.GetCorrelationId());
            sagaStateRepo.FindByCorrelationIdAsync(message.GetCorrelationId())
                .Returns(state); 

            var sut = new SagaRunner<DummySaga, DummySagaState>(sagaFactory, sagaStateFactory, sagaStateRepo);
            
            var messageContext = NSubstitute.Substitute.For<IMessageContext<StartDummySaga>>();
            messageContext.Message.Returns(message);

            await Assert.ThrowsAsync<SagaNotFoundException>(() => sut.RunAsync(messageContext, CancellationToken.None));
        }

        [Fact]
        public async Task RunAsync_should_execute_all_registered_sagas()
        {
            var message = new StartDummySaga(Guid.NewGuid());
            
            var sagaStateFactory = NSubstitute.Substitute.For<ISagaStateFactory<DummySagaState>>();
            var sagaStateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();

            var state = new DummySagaState(message.GetCorrelationId());
            sagaStateRepo.FindByCorrelationIdAsync(message.GetCorrelationId())
                .Returns(state);

            var publisher = NSubstitute.Substitute.For<IMessageBus>();
            var saga = NSubstitute.Substitute.ForPartsOf<DummySaga>(publisher);
            saga.When(s => s.HandleAsync(Arg.Any<IMessageContext<StartDummySaga>>(), Arg.Any<CancellationToken>()))
                .DoNotCallBase();

            var sagaFactory = NSubstitute.Substitute.For<ISagaFactory<DummySaga, DummySagaState>>();
            sagaFactory.Create(state)
                .Returns(saga);

            var sut = new SagaRunner<DummySaga, DummySagaState>(sagaFactory, sagaStateFactory, sagaStateRepo);

            var messageContext = NSubstitute.Substitute.For<IMessageContext<StartDummySaga>>();
            messageContext.Message.Returns(message);

            await sut.RunAsync(messageContext, CancellationToken.None);

            await saga.Received(1)
                .HandleAsync(messageContext, Arg.Any<CancellationToken>());

            await sagaStateRepo.Received(1)
                .SaveAsync(message.GetCorrelationId(), state);
        }
    }
}