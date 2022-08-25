namespace SqlServer.Logging
{
    public interface INLogVariablesResolver
    {
        string Resolve(string name);
    }
}
