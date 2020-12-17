using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Highway.Core.Tests
{
    public class BusConfiguratorTests
    {
        [Fact]
        public void AddConsumer_should_fail_if_another_command_handler_already_registered()
        {
            var services = new ServiceCollection();

            services.AddHighway(cfg =>
            {
                cfg.AddConsumer<DummyCommandConsumer<DummyCommand>, DummyCommand>();
                Assert.Throws<TypeLoadException>(() => cfg.AddConsumer<DummyCommandConsumer<DummyCommand>, DummyCommand>());
            });
        }

        [Fact]
        public void AddConsumer_should_fail_if_another_saga_handles_the_same_command()
        {
            var services = new ServiceCollection();

            services.AddHighway(cfg =>
            {
                cfg.AddSaga<DummySaga, DummySagaState>();
                Assert.Throws<TypeLoadException>(() => cfg.AddConsumer<DummyCommandConsumer<StartDummySaga>, StartDummySaga>());
            });
        }
    }

    internal record DummyCommand(Guid Id) : ICommand
    {
        public Guid GetCorrelationId() => this.Id;
    }

    internal class DummyCommandConsumer<TC> : IHandleMessage<TC>
        where TC : ICommand
    {
        public Task HandleAsync(IMessageContext<TC> context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
    
}
