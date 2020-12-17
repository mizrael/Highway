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
                cfg.AddConsumer<DummyCommandConsumer, DummyCommand>();
                Assert.Throws<TypeLoadException>(() => cfg.AddConsumer<DummyCommandConsumer, DummyCommand>());
            });
        }
    }

    internal record DummyCommand(Guid Id) : ICommand
    {
        public Guid GetCorrelationId() => this.Id;
    }
    
    internal class DummyCommandConsumer : IHandleMessage<DummyCommand>
    {
        public Task HandleAsync(IMessageContext<DummyCommand> context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
