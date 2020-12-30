using Highway.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Highway.Core.Tests
{
    public class BusConfiguratorTests
    {
        [Fact]
        public void AddSaga_should_fail_if_another_saga_handles_the_same_command()
        {
            var services = new ServiceCollection();

            services.AddHighway(cfg =>
            {
                cfg.AddSaga<DummySaga, DummySagaState>();

                Assert.Throws<TypeLoadException>(() => cfg.AddSaga<DummySaga, DummySagaState>());
            });
        }
    }
}
