using Microsoft.Extensions.DependencyInjection;

namespace Highway.Core.DependencyInjection
{
    public interface IBusConfigurator
    {
        ISagaConfigurator<TS, TD> AddSaga<TS, TD>() 
            where TS : Saga<TD>
            where TD : ISagaState;

        IServiceCollection Services { get; }

        IBusConfigurator AddConsumer<TC, TM>()
            where TM : IMessage
            where TC : class, IHandleMessage<TM>;
    }
}