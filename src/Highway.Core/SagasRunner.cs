using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Highway.Core.DependencyInjection;
using Highway.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

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

            var stateTypes = _stateTypeResolver.Resolve<TM>();
            if (!stateTypes.Any())
                throw new SagaNotFoundException($"no saga registered for message of type '{typeof(TM).FullName}'");

            foreach (var (sagaType, stateType) in stateTypes)
            {
                var runnerType = _typesCache.GetGeneric(typeof(ISagaRunner<,>), sagaType, stateType);
                var runner = _serviceProvider.GetService(runnerType);
                if(null == runner)
                    throw new SagaNotFoundException($"no saga registered on DI for message of type '{typeof(TM).FullName}'");

                var genericHandlerMethod = _typesCache.GetMethod(runnerType, nameof(ISagaRunner<Saga<ISagaState>, ISagaState>.RunAsync));
                var handlerMethod = genericHandlerMethod.MakeGenericMethod(typeof(TM));

                var t = handlerMethod.Invoke(runner, new[] { (object)messageContext, (object)cancellationToken }) as Task;
                await t;
            }
        }
    }
}