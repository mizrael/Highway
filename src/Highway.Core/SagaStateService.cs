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
        private readonly IUnitOfWork _uow;
        private readonly IMessageBus _bus;
        
        public SagaStateService(ISagaStateFactory<TD> sagaStateFactory, IUnitOfWork uow, IMessageBus publisher)
        {
            _sagaStateFactory = sagaStateFactory ?? throw new ArgumentNullException(nameof(sagaStateFactory));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _bus = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task<TD> GetAsync<TM>(IMessageContext<TM> messageContext,
                                            ITransaction transaction = null,
                                            CancellationToken cancellationToken = default) where TM : IMessage
        {
            var correlationId = messageContext.Message.GetCorrelationId();
            var state = await _uow.SagaStatesRepository.FindByCorrelationIdAsync<TD>(correlationId, transaction, cancellationToken);

            if (null == state) 
            {
                // if state is null, we're probably starting a new saga.
                // We have to check if the current message can
                // actually start the specified saga or not
                if (!typeof(IStartedBy<TM>).IsAssignableFrom(typeof(TS)))
                    throw new StateCreationException(typeof(TD), $"saga cannot be started by message '{typeof(TM).FullName}'");

                state = _sagaStateFactory.Create(messageContext.Message);
                await _uow.SagaStatesRepository.SaveAsync(correlationId, state, transaction, cancellationToken);
            }

            if (null == state)
                throw new StateCreationException(typeof(TD), "unable to create saga state instance");
            return state;
        }
        
        public async Task SaveAsync(Guid correlationId, TD state, ITransaction transaction = null, CancellationToken cancellationToken = default)
        {
            await _uow.SagaStatesRepository.SaveAsync(correlationId, state, transaction, cancellationToken);
            
            var exceptions = await state.ProcessOutboxAsync(_bus, cancellationToken);

            await _uow.SagaStatesRepository.SaveAsync(correlationId, state, transaction, cancellationToken);

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }
    }
}