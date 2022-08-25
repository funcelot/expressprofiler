namespace SqlServer.DependencyInjection
{
    public interface IServiceScopeFactory
    {
        IServiceScope CreateScope();
    }
}
