using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Highway.Core
{
    public class SagasRunner : ISagasRunner
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISagaTypeResolver _stateTypeResolver;
        private readonly ITypesCache _typesCache;

        public SagasRunner(IServiceProvider serviceProvider, ISagaTypeResolver stateTypeResolver, ITypesCache typesCache)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _stateTypeResolver = stateTypeResolver ?? throw new ArgumentNullException(nameof(stateTypeResolver));
            _typesCache = typesCache ?? throw new ArgumentNullException(nameof(typesCache));
        }

        public async Task RunAsync<TM>(IMessageContext<TM> messageContext, CancellationToken cancellationToken = default)
            where TM : IMessage
        {
            if (messageContext == null)
                throw new ArgumentNullException(nameof(messageContext));

            var types = _stateTypeResolver.Resolve<TM>();
            if (default == types)
                throw new SagaNotFoundException($"no saga registered for message of type '{typeof(TM).FullName}'");

            var runnerType = _typesCache.GetGeneric(typeof(ISagaRunner<,>), types.sagaType, types.sagaStateType);
            var runner = _serviceProvider.GetService(runnerType);
            if (null == runner)
                throw new SagaNotFoundException($"no saga registered on DI for message of type '{typeof(TM).FullName}'");

            var genericHandlerMethod = _typesCache.GetMethod(runnerType, nameof(ISagaRunner<Saga<SagaState>, SagaState>.RunAsync));
            var handlerMethod = genericHandlerMethod.MakeGenericMethod(typeof(TM));

            await (Task)handlerMethod.Invoke(runner, new[] { (object)messageContext, (object)cancellationToken });
        }
    }
}