using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Highway.Core.DependencyInjection
{
    public class SagaTypeResolver : ISagaTypeResolver
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Type>> _types = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Type>>();
        
        public IEnumerable<(Type sagaType, Type sagaStateType)> Resolve<TM>() where TM : IMessage
        {
            var messageType = typeof(TM);

            var sagaStateTypes = _types.GetOrAdd(messageType, new ConcurrentDictionary<Type, Type>());

            foreach (var (key, value) in sagaStateTypes)
                yield return (key, value);
        }

        public void Register(Type messageType, (Type sagaType, Type sagaStateType) types)
        {
            if (!_types.ContainsKey(messageType))
                _types.AddOrUpdate(messageType, new ConcurrentDictionary<Type, Type>(), (k, v) => v);
            if (!_types[messageType].ContainsKey(types.sagaType))
                _types[messageType].AddOrUpdate(types.sagaType, types.sagaStateType, (k, v) => v);
        }
    }
}