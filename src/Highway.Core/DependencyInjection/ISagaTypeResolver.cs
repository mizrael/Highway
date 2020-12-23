using System;

namespace Highway.Core.DependencyInjection
{
    public interface ISagaTypeResolver
    {
        (Type sagaType, Type sagaStateType) Resolve<TM>() where TM : IMessage;
        void Register(Type messageType, (Type sagaType, Type sagaStateType) types);
    }
}