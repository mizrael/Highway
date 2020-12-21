using System;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Core.DependencyInjection
{
    public interface ISagaConfigurator<TS, in TD>
        where TS : Saga<TD> 
        where TD : SagaState
    {
        IServiceCollection Services { get; }
        ISagaConfigurator<TS, TD> UseStateFactory(Func<IMessage, TD> stateFactory);  //TODO: add default when registering the saga
    }

    internal class SagaConfigurator<TS, TD> : ISagaConfigurator<TS, TD>
        where TS : Saga<TD> 
        where TD : SagaState
    {
        public SagaConfigurator(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
        
        public ISagaConfigurator<TS, TD> UseStateFactory(Func<IMessage, TD> stateFactory)
        {
            var stateType = typeof(TD);
            var factory = new LambdaSagaStateFactory<TD>(stateFactory);
            this.Services.AddSingleton(typeof(ISagaStateFactory<>).MakeGenericType(stateType),
                                    factory);
            return this;
        }
    }
}