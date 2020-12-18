using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using Highway.Core.Persistence;

namespace Highway.Core
{
    public class SagaRunner<TS, TD> : ISagaRunner<TS, TD>
        where TS : Saga<TD>
        where TD : ISagaState
    {
        private readonly ISagaStateFactory<TD> _sagaStateFactory;
        private readonly ISagaFactory<TS, TD> _sagaFactory;
        private readonly ISagaStateRepository<TD> _stateRepo;

        public SagaRunner(ISagaFactory<TS, TD> sagaFactory,
            ISagaStateFactory<TD> sagaStateFactory,
            ISagaStateRepository<TD> stateRepo)
        {
            _sagaFactory = sagaFactory ?? throw new ArgumentNullException(nameof(sagaFactory));
            _sagaStateFactory = sagaStateFactory ?? throw new ArgumentNullException(nameof(sagaStateFactory));
            _stateRepo = stateRepo ?? throw new ArgumentNullException(nameof(stateRepo));
        }

        public async Task RunAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken)
            where TM : IMessage
        {
            var correlationId = messageContext.Message.GetCorrelationId();

            var state = await _stateRepo.FindByCorrelationIdAsync(correlationId) ?? _sagaStateFactory.Create();
            if (null == state)
                throw new StateCreationException(typeof(TD), "unable to create state instance");
            
            var saga = _sagaFactory.Create(state);
            if (null == saga)
                throw new SagaNotFoundException($"unable to create Saga of type '{typeof(TS).FullName}'");

            if (saga is not IHandleMessage<TM> handler)
                throw new ConsumerNotFoundException(typeof(TM));

            await handler.HandleAsync(messageContext, cancellationToken);
            
            await _stateRepo.SaveAsync(correlationId, state);
        }
    }
}