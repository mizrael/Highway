using System;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using Highway.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Highway.Core
{
    public class MessageProcessor
    {
        private readonly ISagaTypeResolver _stateTypeResolver;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITypesCache _typesCache;
        private readonly IMessageContextFactory _messageContextFactory;

        public MessageProcessor(IServiceProvider serviceProvider, ISagaTypeResolver stateTypeResolver, 
            ITypesCache typesCache, IMessageContextFactory messageContextFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _stateTypeResolver = stateTypeResolver ?? throw new ArgumentNullException(nameof(stateTypeResolver));
            _typesCache = typesCache ?? throw new ArgumentNullException(nameof(typesCache));
            _messageContextFactory = messageContextFactory ?? throw new ArgumentNullException(nameof(messageContextFactory));
        }

        public async Task ProcessAsync<TM>(TM message, CancellationToken cancellationToken = default)
            where TM : IMessage
        {
            if (message == null) 
                throw new ArgumentNullException(nameof(message));
            
            var correlationId = message.GetCorrelationId();
       
            var stateTypes = _stateTypeResolver.Resolve(message);
            foreach (var (sagaType, stateType) in stateTypes)
            {
                var stateRepoType = _typesCache.GetGeneric(typeof(ISagaStateRepository<>), stateType);
                var stateRepo = _serviceProvider.GetRequiredService(stateRepoType);
                
                var state = await BuildStateAsync(stateType, correlationId, stateRepo, stateRepoType);
                if (null == state)
                    throw new StateCreationException(stateType, "unable to create state instance");
                
                var saga = CreateSaga<TM>(sagaType, stateType, state);

                if (null == saga)
                    throw new SagaNotFoundException(sagaType);
                
                await HandleMessageAsync(message, cancellationToken, sagaType, saga);

                await SaveStateAsync(stateRepoType, stateRepo, correlationId, state);
            }
        }

        private async Task SaveStateAsync(Type stateRepoType, object stateRepo, Guid correlationId, object state) 
        {
            var saveStateMethod = _typesCache.GetMethod(stateRepoType, nameof(ISagaStateRepository<ISagaState>.SaveAsync));
            var saveStateTask = saveStateMethod.Invoke(stateRepo, new[] {(object)correlationId, (object) state}) as Task;
            await Task.WhenAll(saveStateTask);
        }

        private async Task HandleMessageAsync<TM>(TM message, CancellationToken cancellationToken, Type sagaType,
            object saga) where TM : IMessage
        {
            var msgCtx = _messageContextFactory.Create(message);
            
            var msgCtxType = msgCtx.GetType();
            var handleMethod = _typesCache.GetMethod(sagaType, nameof(IHandleMessage<IMessage>.HandleAsync), new[]
            {
                msgCtxType,
                typeof(CancellationToken)
            });
            
            var handleTask = handleMethod.Invoke(saga, new[] {(object)msgCtx, (object) cancellationToken}) as Task;
            await Task.WhenAll(handleTask);
        }

        private object CreateSaga<TM>(Type sagaType, Type stateType, object state) where TM : IMessage
        {
            var sagaFactoryType = _typesCache.GetGeneric(typeof(ISagaFactory<,>), sagaType, stateType);
            
            var sagaFactory = _serviceProvider.GetRequiredService(sagaFactoryType);
            var createSagaMethod = _typesCache.GetMethod(sagaFactoryType, nameof(ISagaFactory<Saga<ISagaState>, ISagaState>.Create),
                new[]
                {
                    stateType
                });
            var saga = createSagaMethod.Invoke(sagaFactory, new[] {state});
            return saga;
        }

        private async Task<object> BuildStateAsync(Type stateType, Guid correlationId, object stateRepo, Type stateRepoType)
        {
            var findStateMethod = _typesCache.GetMethod(stateRepoType, nameof(ISagaStateRepository<ISagaState>.FindByCorrelationIdAsync));
            var findStateTask = findStateMethod.Invoke(stateRepo, new[] {(object) correlationId}) as Task;
            await Task.WhenAll(findStateTask);

            var taskResultProp = _typesCache.GetProperty(findStateTask.GetType(), nameof(Task<ISagaState>.Result));
            var state = taskResultProp.GetValue(findStateTask);
            if (null != state) 
                return state;
            
            var stateFactoryType = _typesCache.GetGeneric(typeof(ISagaStateFactory<>), stateType);
            var sagaStateFactory = _serviceProvider.GetRequiredService(stateFactoryType);
            var createStateMethod = _typesCache.GetMethod(stateFactoryType, nameof(ISagaStateFactory<ISagaState>.Create));
            state = createStateMethod.Invoke(sagaStateFactory, null);

            return state;
        }
    }
}