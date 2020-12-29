using System;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IUnitOfWork _unitOfWork;
        
        public SagaRunner(ISagaFactory<TS, TD> sagaFactory,
                          ISagaStateService<TS, TD> sagaStateService, 
                          IUnitOfWork unitOfWork)
        {
            _sagaFactory = sagaFactory ?? throw new ArgumentNullException(nameof(sagaFactory));
            _sagaStateService = sagaStateService ?? throw new ArgumentNullException(nameof(sagaStateService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task RunAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken)
            where TM : IMessage
        {
            // TODO: consider adding history of processed messages
            // TODO: if a saga instance has to wait to enter the lock, check if the message was processed already
            
            var done = false;
            var random = new Random();
            TD state = null;
            Guid lockId = Guid.Empty;
            while (!done) // TODO: better retry policy (max retries? Polly?)
            {
                try
                {
                    (state, lockId) = await _sagaStateService.GetAsync(messageContext, cancellationToken);

                    done = true;
                }
                catch (LockException)
                {
                    //TODO: logging
                    await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 10)), cancellationToken).ConfigureAwait(false);
                }
            }
            
            var saga = _sagaFactory.Create(state);
            if (null == saga)
                throw new SagaNotFoundException($"unable to create Saga of type '{typeof(TS).FullName}'");

            if (saga is not IHandleMessage<TM> handler)
                throw new ConsumerNotFoundException(typeof(TM));

            //TODO: add configurable retry policy
            await handler.HandleAsync(messageContext, cancellationToken);

            await _sagaStateService.SaveAsync(state, lockId, cancellationToken);

        }
    }
}