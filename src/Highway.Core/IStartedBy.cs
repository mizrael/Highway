namespace Highway.Core
{
    public interface IStartedBy<in TM> : IHandleMessage<TM>
        where TM : IMessage
    { }
}