using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Highway.Core.DependencyInjection
{
    public class SagaTypeResolver : ISagaTypeResolver
    {
        private readonly ConcurrentDictionary<Type, (Type, Type)> _types = new ConcurrentDictionary<Type, (Type, Type)>();
        
        public (Type sagaType, Type sagaStateType) Resolve<TM>() where TM : IMessage
        {
            var messageType = typeof(TM);

            _types.TryGetValue(messageType, out var types);
            return types;
        }

        public void Register(Type messageType, (Type sagaType, Type sagaStateType) types)
        {
            if (_types.ContainsKey(messageType))
                throw new TypeAccessException($"there is already a saga for message type '{messageType.FullName}'");

            _types.AddOrUpdate(messageType, types, (k,v) => types);
        }

        public IReadOnlyCollection<Type> GetMessageTypes() => _types.Keys.ToImmutableList();
    }
}