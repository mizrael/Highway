using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Highway.Core.Tests
{
    public class SagasRunnerTests
    {
        [Fact]
        public async Task RunAsync_should_throw_if_no_saga_registered()
        {
            var typesCache = NSubstitute.Substitute.For<ITypesCache>();
            var stateTypeResolver = NSubstitute.Substitute.For<ISagaTypeResolver>();
            var sp = NSubstitute.Substitute.For<IServiceProvider>();

            var sut = new SagasRunner(sp, stateTypeResolver, typesCache);

            var message = StartDummySaga.New();
            var messageContext = NSubstitute.Substitute.For<IMessageContext<StartDummySaga>>();
            messageContext.Message.Returns(message);

            await Assert.ThrowsAsync<SagaNotFoundException>(() => sut.RunAsync(messageContext));
        }

        [Fact]
        public async Task RunAsync_should_throw_SagaNotFoundException_if_no_saga_registered_on_DI()
        {
            var stateTypeResolver = NSubstitute.Substitute.For<ISagaTypeResolver>();
            var types = (typeof(DummySaga), typeof(DummySagaState));
            stateTypeResolver.Resolve<StartDummySaga>()
                .Returns(types);

            var sp = NSubstitute.Substitute.For<IServiceProvider>();

            var typesCache = NSubstitute.Substitute.For<ITypesCache>();

            var sut = new SagasRunner(sp, stateTypeResolver, typesCache);

            var message = StartDummySaga.New();
            var messageContext = NSubstitute.Substitute.For<IMessageContext<StartDummySaga>>();
            messageContext.Message.Returns(message);

            await Assert.ThrowsAsync<SagaNotFoundException>(() => sut.RunAsync(messageContext));
        }
    }
}