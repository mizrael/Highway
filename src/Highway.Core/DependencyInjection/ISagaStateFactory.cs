namespace Highway.Core.DependencyInjection
{
    public interface ISagaStateFactory<TD>
    {
        TD Create();
    }
}