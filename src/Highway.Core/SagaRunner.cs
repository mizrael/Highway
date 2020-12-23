using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using Highway.Core.Persistence;

namespace Highway.Core
{
    public class SagaRunner<TS, TD> : ISagaRunner<TS, TD>
        where TS : Saga<TD>
        where TD : SagaState
    {
        private readonly ISagaStateService<TS, TD> _sagaStateService;
        private readonly ISagaFactory<TS, TD> _sagaFactory;
        
        public SagaRunner(ISagaFactory<TS, TD> sagaFactory,
                          ISagaStateService<TS, TD> sagaStateService)
        {
            _sagaFactory = sagaFactory ?? throw new ArgumentNullException(nameof(sagaFactory));
            _sagaStateService = sagaStateService ?? throw new ArgumentNullException(nameof(sagaStateService));
        }

        public async Task RunAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken)
            where TM : IMessage
        {
            var state = await _sagaStateService.GetAsync(messageContext, cancellationToken);

            // TODO: add lock on state to prevent concurrency issues
            // TODO: consider adding history of processed messages
            // TODO: if a saga instance has to wait to enter the lock, check if the message was processed already
            
            var saga = _sagaFactory.Create(state);
            if (null == saga)
                throw new SagaNotFoundException($"unable to create Saga of type '{typeof(TS).FullName}'");

            if (saga is not IHandleMessage<TM> handler)
                throw new ConsumerNotFoundException(typeof(TM));

            await handler.HandleAsync(messageContext, cancellationToken);

            var correlationId = messageContext.Message.GetCorrelationId();
            await _sagaStateService.SaveAsync(correlationId, state, cancellationToken);
        }

    }
}