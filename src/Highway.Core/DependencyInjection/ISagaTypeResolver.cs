using System;
using System.Collections.Generic;

namespace Highway.Core.DependencyInjection
{
    public interface ISagaTypeResolver
    {
        IEnumerable<(Type sagaType, Type sagaStateType)> Resolve<TM>(TM message) where TM : IMessage;
        void Register(Type messageType, (Type sagaType, Type sagaStateType) types);
    }
}