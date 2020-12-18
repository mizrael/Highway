namespace Highway.Core.DependencyInjection
{
    public interface ISagaStateFactory<out TD>
        where TD : ISagaState
    {
        TD Create(IMessage message);
    }
}