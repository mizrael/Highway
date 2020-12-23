using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using Highway.Core.Persistence;

namespace Highway.Core
{
    public class SagaStateService<TS, TD> : ISagaStateService<TS, TD> 
        where TS : Saga<TD>
        where TD : SagaState
    {
        private readonly ISagaStateFactory<TD> _sagaStateFactory;
        private readonly ISagaStateRepository<TD> _stateRepo;
        private readonly IMessageBus _publisher;
        
        public SagaStateService(ISagaStateFactory<TD> sagaStateFactory, ISagaStateRepository<TD> stateRepo, IMessageBus publisher)
        {
            _sagaStateFactory = sagaStateFactory ?? throw new ArgumentNullException(nameof(sagaStateFactory));
            _stateRepo = stateRepo ?? throw new ArgumentNullException(nameof(stateRepo));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task<TD> GetAsync<TM>(IMessageContext<TM> messageContext,
                                            CancellationToken cancellationToken = default) where TM : IMessage
        {
            var state = await _stateRepo.FindByCorrelationIdAsync(messageContext.Message.GetCorrelationId(), cancellationToken);

            if (null == state) 
            {
                // if state is null, we're probably starting a new saga.
                // We have to check if the current message can
                // actually start the specified saga or not
                if (!typeof(IStartedBy<TM>).IsAssignableFrom(typeof(TS)))
                    throw new StateCreationException(typeof(TD), $"saga cannot be started by message '{typeof(TM).FullName}'");

                state = _sagaStateFactory.Create(messageContext.Message);
            }

            if (null == state)
                throw new StateCreationException(typeof(TD), "unable to create saga state instance");
            return state;
        }
        
        public async Task SaveAsync(Guid correlationId, TD state, CancellationToken cancellationToken)
        {
            await _stateRepo.SaveAsync(correlationId, state, cancellationToken);

            // TODO: make sure state locks don't cause issue here (eg. loopback messages cannot be processed)
            var exceptions = await state.ProcessOutboxAsync(_publisher, cancellationToken);

            await _stateRepo.SaveAsync(correlationId, state, cancellationToken);

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }
    }
}