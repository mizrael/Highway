namespace Highway.Core
{
    public interface ISagaFactory<TS,TD>
        where TD : ISagaState
        where TS : Saga<TD>
    {
        TS Create(TD state);
    }
}