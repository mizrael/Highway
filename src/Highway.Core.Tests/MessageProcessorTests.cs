using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Highway.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Xunit;

namespace Highway.Core.Tests
{
    // find saga by correlation id
    // found:
    //      check can handle message by message type
    //      yes -> handle message
    //      no:
    //          search for a saga type that can be started by the message
    //          found: create instance and handle message
    //          not found: throw
    // not found:
    //      search for a saga type that can be started by the message
    //      found: create instance and handle message
    //      not found: throw

    
    public class MessageProcessorTests
    {
        [Fact]
        public async Task ProcessAsync_should_process_message_when_saga_registered()
        {
            var services = new ServiceCollection();

            var stateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();
            services.AddSingleton<ISagaStateRepository<DummySagaState>>(stateRepo);

            var publisher = NSubstitute.Substitute.For<IPublisher>();
            services.AddSingleton<IPublisher>(publisher);

            services.AddHighway(cfg =>
            {
                cfg.RegisterSaga<DummySaga, DummySagaState>()
                    .UseStateFactory(DummySagaState.Empty);
            });

            var saga = NSubstitute.Substitute.ForPartsOf<DummySaga>(publisher);
            saga.WhenForAnyArgs(s => s.HandleAsync(Arg.Any<IMessageContext<DummySagaStarter>>(), Arg.Any<CancellationToken>()))
                .DoNotCallBase();
            saga.WhenForAnyArgs(s => s.HandleAsync(Arg.Any<IMessageContext<DummySagaStarted>>(), Arg.Any<CancellationToken>()))
                .DoNotCallBase();

            var sagaFactory = NSubstitute.Substitute.For<ISagaFactory<DummySaga, DummySagaState>>();
            sagaFactory.Create(Arg.Any<DummySagaState>())
                .Returns(saga);
            services.Replace(ServiceDescriptor.Singleton<ISagaFactory<DummySaga, DummySagaState>>(sagaFactory));

            /*****************************************************************/

            var sut = CreateSut(services);

            var message = new DummySagaStarter(Guid.NewGuid());
            await sut.ProcessAsync(message, CancellationToken.None);

            await saga.Received(1).HandleAsync(Arg.Any<IMessageContext<DummySagaStarter>>(), Arg.Any<CancellationToken>());
            await saga.Received(0).HandleAsync(Arg.Any<IMessageContext<DummySagaStarted>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAsync_should_persist_state()
        {
            var services = new ServiceCollection();

            var publisher = NSubstitute.Substitute.For<IPublisher>();
            services.AddSingleton<IPublisher>(publisher);

            services.AddHighway(cfg =>
            {
                cfg.RegisterSaga<DummySaga, DummySagaState>()
                    .UseStateFactory(DummySagaState.Empty);
            });

            var saga = NSubstitute.Substitute.ForPartsOf<DummySaga>(publisher);
            saga.WhenForAnyArgs(s => s.HandleAsync(Arg.Any<IMessageContext<DummySagaStarter>>(), Arg.Any<CancellationToken>()))
                .DoNotCallBase();

            var sagaFactory = NSubstitute.Substitute.For<ISagaFactory<DummySaga, DummySagaState>>();
            sagaFactory.Create(Arg.Any<DummySagaState>())
                .Returns(saga);
            services.Replace(ServiceDescriptor.Singleton<ISagaFactory<DummySaga, DummySagaState>>(sagaFactory));

            var stateRepo = NSubstitute.Substitute.For<ISagaStateRepository<DummySagaState>>();
            services.Replace(ServiceDescriptor.Singleton<ISagaStateRepository<DummySagaState>>(stateRepo));

            /*****************************************************************/

            var sut = CreateSut(services);

            var message = new DummySagaStarter(Guid.NewGuid());
            await sut.ProcessAsync(message, CancellationToken.None);

            await stateRepo.Received(1)
                .SaveAsync(message.GetCorrelationId(), Arg.Any<DummySagaState>());
        }

        private static MessageProcessor CreateSut(IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();

            var stateTypeResolver = sp.GetRequiredService<ISagaTypeResolver>();
            var typesCache = sp.GetRequiredService<ITypesCache>();
            var msgCtxFactory = sp.GetRequiredService<IMessageContextFactory>();

            var sut = new MessageProcessor(sp, stateTypeResolver, typesCache, msgCtxFactory);
            return sut;
        }

    }
}
