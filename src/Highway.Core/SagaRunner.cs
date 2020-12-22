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
        private readonly ISagaStateFactory<TD> _sagaStateFactory;
        private readonly ISagaFactory<TS, TD> _sagaFactory;
        private readonly ISagaStateRepository<TD> _stateRepo;
        private readonly IMessageBus _publisher;
        
        public SagaRunner(ISagaFactory<TS, TD> sagaFactory,
            ISagaStateFactory<TD> sagaStateFactory,
            ISagaStateRepository<TD> stateRepo, 
            IMessageBus publisher)
        {
            _sagaFactory = sagaFactory ?? throw new ArgumentNullException(nameof(sagaFactory));
            _sagaStateFactory = sagaStateFactory ?? throw new ArgumentNullException(nameof(sagaStateFactory));
            _stateRepo = stateRepo ?? throw new ArgumentNullException(nameof(stateRepo));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task RunAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken)
            where TM : IMessage
        {
            var correlationId = messageContext.Message.GetCorrelationId();

            var state = await _stateRepo.FindByCorrelationIdAsync(correlationId);

            if (null == state) //TODO: add test
            {
                // if state is null, means we're starting a new saga. We have to check if the current message can
                // actually start the specified saga or not
                if (typeof(IStartedBy<TM>).IsAssignableFrom(typeof(TS)))
                    state = _sagaStateFactory.Create(messageContext.Message);
            }

            if (null == state)
                throw new StateCreationException(typeof(TD), "unable to create state instance");

            // TODO: add lock on state to prevent concurrency issues
            // TODO: consider adding history of processed messages
            // TODO: if a saga instance has to wait to enter the lock, check if the message was processed already
            
            var saga = _sagaFactory.Create(state);
            if (null == saga)
                throw new SagaNotFoundException($"unable to create Saga of type '{typeof(TS).FullName}'");

            if (saga is not IHandleMessage<TM> handler)
                throw new ConsumerNotFoundException(typeof(TM));

            await handler.HandleAsync(messageContext, cancellationToken);

            await _stateRepo.SaveAsync(correlationId, state);

            // TODO: make sure state locks don't cause issue here (eg. loopback messages cannot be processed)
            var exceptions = await state.ProcessOutboxAsync(_publisher, cancellationToken);

            await _stateRepo.SaveAsync(correlationId, state);
            
            if(exceptions.Any())
                throw new AggregateException(exceptions);
        }
    }
}