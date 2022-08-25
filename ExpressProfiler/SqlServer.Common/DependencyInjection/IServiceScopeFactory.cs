namespace Express.DependencyInjection
{
    public interface IServiceScopeFactory
    {
        IServiceScope CreateScope();
    }
}
